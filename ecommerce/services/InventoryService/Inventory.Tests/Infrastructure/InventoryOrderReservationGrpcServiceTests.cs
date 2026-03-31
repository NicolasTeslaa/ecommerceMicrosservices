using ECommerce.Shared.Protos;
using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Grpc;
using Inventory.Tests.Support;
using Moq;

namespace Inventory.Tests.Infrastructure;

public class InventoryOrderReservationGrpcServiceTests
{
    [Fact]
    public async Task ReserveOrderItems_ShouldFail_WhenOrderOrCustomerIdIsInvalid()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);

        var reply = await service.ReserveOrderItems(
            new ReserveOrderItemsRequest
            {
                OrderId = "invalid",
                CustomerId = Guid.NewGuid().ToString()
            },
            new TestServerCallContext());

        Assert.False(reply.IsSuccess);
        Assert.Equal("Order or customer identifier is invalid.", reply.Reason);
        repository.Verify(item => item.GetReservationsByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReserveOrderItems_ShouldReturnSuccess_WhenInventoryWasAlreadyReserved()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        repository.Setup(item => item.GetReservationsByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { InventoryTestData.CreateReservation() });

        var reply = await service.ReserveOrderItems(CreateReserveRequest(), new TestServerCallContext());

        Assert.True(reply.IsSuccess);
        Assert.Equal("Inventory was already reserved for this order.", reply.Reason);
        repository.Verify(item => item.AddReservationsAsync(It.IsAny<IEnumerable<InventoryReservation>>(), It.IsAny<CancellationToken>()), Times.Never);
        eventPublisher.Verify(item => item.PublishReservationRejectedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<InventoryReservationIssueDto>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReserveOrderItems_ShouldAggregateDuplicateProductLines_IntoSingleReservation()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var inventoryItem = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 10);
        List<InventoryReservation>? addedReservations = null;
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryReservation>());
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { inventoryItem });
        repository.Setup(item => item.AddReservationsAsync(It.IsAny<IEnumerable<InventoryReservation>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<InventoryReservation>, CancellationToken>((reservations, _) => addedReservations = reservations.ToList())
            .Returns(Task.CompletedTask);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var reply = await service.ReserveOrderItems(
            new ReserveOrderItemsRequest
            {
                OrderId = orderId.ToString(),
                CustomerId = customerId.ToString(),
                Items =
                {
                    new ReserveOrderItem { ProductId = productId.ToString(), ProductName = "Produto A", Quantity = 2 },
                    new ReserveOrderItem { ProductId = productId.ToString(), ProductName = "Produto A", Quantity = 3 }
                }
            },
            new TestServerCallContext());

        Assert.True(reply.IsSuccess);
        Assert.NotNull(addedReservations);
        Assert.Single(addedReservations!);
        Assert.Equal(5, addedReservations[0].Quantity);
        Assert.Equal(5, inventoryItem.AvailableQuantity);
        Assert.Equal(5, inventoryItem.ReservedQuantity);
    }

    [Fact]
    public async Task ReserveOrderItems_ShouldRejectRequest_WhenProductDoesNotExist()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryReservation>());
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryItem>());

        var reply = await service.ReserveOrderItems(
            new ReserveOrderItemsRequest
            {
                OrderId = orderId.ToString(),
                CustomerId = customerId.ToString(),
                Items = { new ReserveOrderItem { ProductId = productId.ToString(), ProductName = "Produto X", Quantity = 2 } }
            },
            new TestServerCallContext());

        Assert.False(reply.IsSuccess);
        Assert.Equal("Product does not exist or is inactive.", reply.Reason);
        Assert.Single(reply.RejectedItems);
        eventPublisher.Verify(item => item.PublishReservationRejectedAsync(
            orderId,
            customerId,
            "Product does not exist or is inactive.",
            It.Is<IReadOnlyCollection<InventoryReservationIssueDto>>(issues => issues.Count == 1 && issues.First().AvailableQuantity == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReserveOrderItems_ShouldRejectRequest_WhenProductIsInactive()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var inventoryItem = InventoryTestData.CreateItem(productId: productId, active: false);
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryReservation>());
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { inventoryItem });

        var reply = await service.ReserveOrderItems(
            new ReserveOrderItemsRequest
            {
                OrderId = orderId.ToString(),
                CustomerId = customerId.ToString(),
                Items = { new ReserveOrderItem { ProductId = productId.ToString(), ProductName = "Produto X", Quantity = 1 } }
            },
            new TestServerCallContext());

        Assert.False(reply.IsSuccess);
        Assert.Equal("Product does not exist or is inactive.", reply.Reason);
    }

    [Fact]
    public async Task ReserveOrderItems_ShouldRejectRequest_WhenStockIsInsufficient()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var inventoryItem = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 1);
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryReservation>());
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { inventoryItem });

        var reply = await service.ReserveOrderItems(
            new ReserveOrderItemsRequest
            {
                OrderId = orderId.ToString(),
                CustomerId = customerId.ToString(),
                Items = { new ReserveOrderItem { ProductId = productId.ToString(), ProductName = "Produto Y", Quantity = 3 } }
            },
            new TestServerCallContext());

        Assert.False(reply.IsSuccess);
        Assert.Equal("Insufficient stock.", reply.Reason);
        Assert.Single(reply.RejectedItems);
        Assert.Equal(1, reply.RejectedItems[0].AvailableQuantity);
    }

    [Fact]
    public async Task ReserveOrderItems_ShouldRejectWithGenericReason_WhenMultipleProductsFail()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        var firstItem = InventoryTestData.CreateItem(productId: firstProductId, initialStockQuantity: 1);
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryReservation>());
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { firstItem });

        var reply = await service.ReserveOrderItems(
            new ReserveOrderItemsRequest
            {
                OrderId = orderId.ToString(),
                CustomerId = customerId.ToString(),
                Items =
                {
                    new ReserveOrderItem { ProductId = firstProductId.ToString(), ProductName = "Produto A", Quantity = 3 },
                    new ReserveOrderItem { ProductId = secondProductId.ToString(), ProductName = "Produto B", Quantity = 2 }
                }
            },
            new TestServerCallContext());

        Assert.False(reply.IsSuccess);
        Assert.Equal("One or more products are unavailable for reservation.", reply.Reason);
        Assert.Equal(2, reply.RejectedItems.Count);
    }

    [Fact]
    public async Task ReserveOrderItems_ShouldReserveValidProducts_WhenRequestContainsInvalidProductIdStrings()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var inventoryItem = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 6);
        List<InventoryReservation>? addedReservations = null;
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryReservation>());
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { inventoryItem });
        repository.Setup(item => item.AddReservationsAsync(It.IsAny<IEnumerable<InventoryReservation>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<InventoryReservation>, CancellationToken>((reservations, _) => addedReservations = reservations.ToList())
            .Returns(Task.CompletedTask);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var reply = await service.ReserveOrderItems(
            new ReserveOrderItemsRequest
            {
                OrderId = orderId.ToString(),
                CustomerId = customerId.ToString(),
                Items =
                {
                    new ReserveOrderItem { ProductId = "not-a-guid", ProductName = "Invalido", Quantity = 5 },
                    new ReserveOrderItem { ProductId = productId.ToString(), ProductName = "Produto Valido", Quantity = 2 }
                }
            },
            new TestServerCallContext());

        Assert.True(reply.IsSuccess);
        Assert.NotNull(addedReservations);
        Assert.Single(addedReservations!);
        Assert.Equal(productId, addedReservations[0].ProductId);
    }

    [Fact]
    public async Task ReserveOrderItems_ShouldPersistReservations_WhenAllProductsAreAvailable()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        var firstItem = InventoryTestData.CreateItem(productId: firstProductId, initialStockQuantity: 10);
        var secondItem = InventoryTestData.CreateItem(productId: secondProductId, initialStockQuantity: 4);
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryReservation>());
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { firstItem, secondItem });
        repository.Setup(item => item.AddReservationsAsync(It.IsAny<IEnumerable<InventoryReservation>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var reply = await service.ReserveOrderItems(
            new ReserveOrderItemsRequest
            {
                OrderId = orderId.ToString(),
                CustomerId = customerId.ToString(),
                Items =
                {
                    new ReserveOrderItem { ProductId = firstProductId.ToString(), ProductName = "Produto 1", Quantity = 3 },
                    new ReserveOrderItem { ProductId = secondProductId.ToString(), ProductName = "Produto 2", Quantity = 1 }
                }
            },
            new TestServerCallContext());

        Assert.True(reply.IsSuccess);
        Assert.Equal("Inventory reserved successfully.", reply.Reason);
        repository.Verify(item => item.AddReservationsAsync(It.Is<IEnumerable<InventoryReservation>>(reservations => reservations.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(7, firstItem.AvailableQuantity);
        Assert.Equal(3, firstItem.ReservedQuantity);
        Assert.Equal(3, secondItem.AvailableQuantity);
        Assert.Equal(1, secondItem.ReservedQuantity);
    }

    [Fact]
    public async Task ReleaseOrderReservation_ShouldFail_WhenOrderIdIsInvalid()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);

        var reply = await service.ReleaseOrderReservation(
            new ReleaseOrderReservationRequest { OrderId = "invalid" },
            new TestServerCallContext());

        Assert.False(reply.IsSuccess);
        Assert.Equal("Order identifier is invalid.", reply.Reason);
    }

    [Fact]
    public async Task ReleaseOrderReservation_ShouldReturnSuccess_WhenNoReservationsExist()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        repository.Setup(item => item.GetReservationsByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryReservation>());

        var reply = await service.ReleaseOrderReservation(
            new ReleaseOrderReservationRequest { OrderId = Guid.NewGuid().ToString() },
            new TestServerCallContext());

        Assert.True(reply.IsSuccess);
        Assert.Equal("No reservations were found for this order.", reply.Reason);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReleaseOrderReservation_ShouldReleaseOnlyPendingReservations()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        var firstItem = InventoryTestData.CreateItem(productId: firstProductId, initialStockQuantity: 10);
        var secondItem = InventoryTestData.CreateItem(productId: secondProductId, initialStockQuantity: 8);
        firstItem.Reserve(2);
        secondItem.Reserve(3);
        secondItem.ConfirmReservation(3);
        var pendingReservation = new InventoryReservation(orderId, firstProductId, 2);
        var confirmedReservation = new InventoryReservation(orderId, secondProductId, 3);
        confirmedReservation.Confirm();
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { pendingReservation, confirmedReservation });
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { firstItem, secondItem });
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var reply = await service.ReleaseOrderReservation(
            new ReleaseOrderReservationRequest { OrderId = orderId.ToString() },
            new TestServerCallContext());

        Assert.True(reply.IsSuccess);
        Assert.Equal(InventoryReservationStatus.Released, pendingReservation.Status);
        Assert.Equal(InventoryReservationStatus.Confirmed, confirmedReservation.Status);
        Assert.Equal(10, firstItem.AvailableQuantity);
        Assert.Equal(0, firstItem.ReservedQuantity);
        Assert.Equal(5, secondItem.AvailableQuantity);
        Assert.Equal(0, secondItem.ReservedQuantity);
    }

    [Fact]
    public async Task ReleaseOrderReservation_ShouldIgnoreMissingInventoryItem()
    {
        var repository = new Mock<IInventoryRepository>();
        var eventPublisher = new Mock<IInventoryEventPublisher>();
        var service = new InventoryOrderReservationGrpcService(repository.Object, eventPublisher.Object);
        var orderId = Guid.NewGuid();
        var reservation = InventoryTestData.CreateReservation(orderId: orderId, quantity: 2);
        repository.Setup(item => item.GetReservationsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { reservation });
        repository.Setup(item => item.GetItemsByProductIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InventoryItem>());
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var reply = await service.ReleaseOrderReservation(
            new ReleaseOrderReservationRequest { OrderId = orderId.ToString() },
            new TestServerCallContext());

        Assert.True(reply.IsSuccess);
        Assert.Equal(InventoryReservationStatus.Pending, reservation.Status);
    }

    private static ReserveOrderItemsRequest CreateReserveRequest()
    {
        return new ReserveOrderItemsRequest
        {
            OrderId = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString()
        };
    }

}
