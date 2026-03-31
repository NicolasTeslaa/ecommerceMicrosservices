using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Exceptions;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence;

namespace Order.Tests.Infrastructure;

public class OrderProcessorConsumerServiceTests
{
    [Fact]
    public async Task ProcessMessageAsync_ShouldPersistRejectedOrder_WhenStockValidationFails()
    {
        var writeDbContext = CreateWriteDbContext();
        var request = CreateOrderProcessingRequest();
        var outbox = await SeedOutboxAsync(writeDbContext, request);

        var readModelProjectorMock = new Mock<IOrderReadModelProjector>();
        var customerAddressClientMock = new Mock<ICustomerAddressValidationClient>();
        var inventoryOrderReservationClientMock = new Mock<IInventoryOrderReservationClient>();
        var eventPublisherMock = new Mock<IOrderEventPublisher>();

        inventoryOrderReservationClientMock
            .Setup(client => client.ReserveAsync(request.OrderId, request.CustomerId, It.IsAny<IReadOnlyCollection<ProductAvailabilityCheckItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductAvailabilityValidationResultDto
            {
                IsValid = false,
                Reason = "One or more products are unavailable for this order.",
                Issues =
                [
                    new ProductAvailabilityIssueDto
                    {
                        ProductId = request.Items.First().ProductId,
                        ProductName = request.Items.First().ProductName,
                        RequestedQuantity = request.Items.First().Quantity,
                        AvailableQuantity = 0,
                        Reason = "Insufficient stock."
                    }
                ]
            });

        var provider = BuildProvider(
            writeDbContext,
            readModelProjectorMock.Object,
            customerAddressClientMock.Object,
            inventoryOrderReservationClientMock.Object,
            eventPublisherMock.Object);

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, outbox.Id);

        var savedOrder = await writeDbContext.Orders.Include(order => order.Items).SingleAsync();
        Assert.Equal(OrderStatus.PaymentRejected, savedOrder.Status);
        Assert.Equal(OrderRejectionReason.InsufficientStock, savedOrder.RejectionReason);
        Assert.Equal(request.OrderId, savedOrder.Id);
        Assert.NotNull(outbox.ProcessedAtUtc);

        eventPublisherMock.Verify(
            publisher => publisher.PublishOrderRejectedAsync(
                request.OrderId,
                request.CustomerId,
                request.CustomerAddressId,
                request.RequestedAtUtc,
                It.IsAny<string>(),
                It.IsAny<IReadOnlyCollection<ProductAvailabilityIssueDto>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        customerAddressClientMock.Verify(
            client => client.ValidateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldPersistRejectedOrderWithProductUnavailable_WhenCatalogReturnsMissingProduct()
    {
        var writeDbContext = CreateWriteDbContext();
        var request = CreateOrderProcessingRequest();
        var outbox = await SeedOutboxAsync(writeDbContext, request);

        var readModelProjectorMock = new Mock<IOrderReadModelProjector>();
        var customerAddressClientMock = new Mock<ICustomerAddressValidationClient>();
        var inventoryOrderReservationClientMock = new Mock<IInventoryOrderReservationClient>();
        var eventPublisherMock = new Mock<IOrderEventPublisher>();

        inventoryOrderReservationClientMock
            .Setup(client => client.ReserveAsync(request.OrderId, request.CustomerId, It.IsAny<IReadOnlyCollection<ProductAvailabilityCheckItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductAvailabilityValidationResultDto
            {
                IsValid = false,
                Reason = "Product no longer exists.",
                Issues =
                [
                    new ProductAvailabilityIssueDto
                    {
                        ProductId = request.Items.First().ProductId,
                        ProductName = request.Items.First().ProductName,
                        RequestedQuantity = request.Items.First().Quantity,
                        AvailableQuantity = 0,
                        Reason = "Product was not found."
                    }
                ]
            });

        var provider = BuildProvider(
            writeDbContext,
            readModelProjectorMock.Object,
            customerAddressClientMock.Object,
            inventoryOrderReservationClientMock.Object,
            eventPublisherMock.Object);

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, outbox.Id);

        var savedOrder = await writeDbContext.Orders.SingleAsync();
        Assert.Equal(OrderStatus.PaymentRejected, savedOrder.Status);
        Assert.Equal(OrderRejectionReason.ProductUnavailable, savedOrder.RejectionReason);
        Assert.NotNull(outbox.ProcessedAtUtc);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldPersistRejectedOrder_WhenAddressValidationFails()
    {
        var writeDbContext = CreateWriteDbContext();
        var request = CreateOrderProcessingRequest();
        var outbox = await SeedOutboxAsync(writeDbContext, request);

        var readModelProjectorMock = new Mock<IOrderReadModelProjector>();
        var customerAddressClientMock = new Mock<ICustomerAddressValidationClient>();
        var inventoryOrderReservationClientMock = new Mock<IInventoryOrderReservationClient>();
        var eventPublisherMock = new Mock<IOrderEventPublisher>();

        inventoryOrderReservationClientMock
            .Setup(client => client.ReserveAsync(request.OrderId, request.CustomerId, It.IsAny<IReadOnlyCollection<ProductAvailabilityCheckItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductAvailabilityValidationResultDto
            {
                IsValid = true,
                Reason = "All items are available."
            });

        customerAddressClientMock
            .Setup(client => client.ValidateAsync(request.CustomerId, request.CustomerAddressId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CustomerAddressNotFoundException(request.CustomerId, request.CustomerAddressId));

        var provider = BuildProvider(
            writeDbContext,
            readModelProjectorMock.Object,
            customerAddressClientMock.Object,
            inventoryOrderReservationClientMock.Object,
            eventPublisherMock.Object);

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, outbox.Id);

        var savedOrder = await writeDbContext.Orders.SingleAsync();
        Assert.Equal(OrderStatus.PaymentRejected, savedOrder.Status);
        Assert.Equal(OrderRejectionReason.InvalidCustomerAddress, savedOrder.RejectionReason);
        Assert.NotNull(outbox.ProcessedAtUtc);

        eventPublisherMock.Verify(
            publisher => publisher.PublishOrderRejectedAsync(
                request.OrderId,
                request.CustomerId,
                request.CustomerAddressId,
                request.RequestedAtUtc,
                It.Is<string>(reason => reason.Contains(request.CustomerAddressId.ToString())),
                It.IsAny<IReadOnlyCollection<ProductAvailabilityIssueDto>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldPersistPendingPaymentOrder_WhenAllValidationsPass()
    {
        var writeDbContext = CreateWriteDbContext();
        var request = CreateOrderProcessingRequest();
        var outbox = await SeedOutboxAsync(writeDbContext, request);

        var readModelProjectorMock = new Mock<IOrderReadModelProjector>();
        var customerAddressClientMock = new Mock<ICustomerAddressValidationClient>();
        var inventoryOrderReservationClientMock = new Mock<IInventoryOrderReservationClient>();
        var eventPublisherMock = new Mock<IOrderEventPublisher>();

        inventoryOrderReservationClientMock
            .Setup(client => client.ReserveAsync(request.OrderId, request.CustomerId, It.IsAny<IReadOnlyCollection<ProductAvailabilityCheckItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductAvailabilityValidationResultDto
            {
                IsValid = true,
                Reason = "All items are available."
            });

        customerAddressClientMock
            .Setup(client => client.ValidateAsync(request.CustomerId, request.CustomerAddressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidatedCustomerAddressDto
            {
                CustomerId = request.CustomerId,
                AddressId = request.CustomerAddressId,
                CustomerEmail = "customer@example.com",
                FormattedAddress = "Rua A, 123"
            });

        var provider = BuildProvider(
            writeDbContext,
            readModelProjectorMock.Object,
            customerAddressClientMock.Object,
            inventoryOrderReservationClientMock.Object,
            eventPublisherMock.Object);

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, outbox.Id);

        var savedOrder = await writeDbContext.Orders.Include(order => order.Items).SingleAsync();
        Assert.Equal(OrderStatus.PendingPayment, savedOrder.Status);
        Assert.Null(savedOrder.RejectionReason);
        Assert.Equal("customer@example.com", savedOrder.CustomerEmail);
        Assert.Equal("Rua A, 123", savedOrder.ShippingAddress);
        Assert.NotNull(outbox.ProcessedAtUtc);

        eventPublisherMock.Verify(
            publisher => publisher.PublishOrderCreatedAsync(
                It.Is<Order.Domain.Entities.Order>(order => order.Id == request.OrderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldReuseExistingOrder_WhenOrderWasAlreadyPersisted()
    {
        var writeDbContext = CreateWriteDbContext();
        var request = CreateOrderProcessingRequest();
        var existingOrder = CreateExistingOrder(request);
        await writeDbContext.Orders.AddAsync(existingOrder);
        var outbox = await SeedOutboxAsync(writeDbContext, request);

        var readModelProjectorMock = new Mock<IOrderReadModelProjector>();
        var customerAddressClientMock = new Mock<ICustomerAddressValidationClient>();
        var inventoryOrderReservationClientMock = new Mock<IInventoryOrderReservationClient>();
        var eventPublisherMock = new Mock<IOrderEventPublisher>();

        inventoryOrderReservationClientMock
            .Setup(client => client.ReserveAsync(request.OrderId, request.CustomerId, It.IsAny<IReadOnlyCollection<ProductAvailabilityCheckItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductAvailabilityValidationResultDto
            {
                IsValid = true,
                Reason = "All items are available."
            });

        var provider = BuildProvider(
            writeDbContext,
            readModelProjectorMock.Object,
            customerAddressClientMock.Object,
            inventoryOrderReservationClientMock.Object,
            eventPublisherMock.Object);

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, outbox.Id);

        Assert.Equal(1, await writeDbContext.Orders.CountAsync());
        Assert.NotNull(outbox.ProcessedAtUtc);

        customerAddressClientMock.Verify(
            client => client.ValidateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        eventPublisherMock.Verify(
            publisher => publisher.PublishOrderCreatedAsync(
                It.Is<Order.Domain.Entities.Order>(order => order.Id == existingOrder.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldRegisterProcessingFailure_WhenProjectorThrows()
    {
        var writeDbContext = CreateWriteDbContext();
        var request = CreateOrderProcessingRequest();
        var outbox = await SeedOutboxAsync(writeDbContext, request);

        var readModelProjectorMock = new Mock<IOrderReadModelProjector>();
        var customerAddressClientMock = new Mock<ICustomerAddressValidationClient>();
        var inventoryOrderReservationClientMock = new Mock<IInventoryOrderReservationClient>();
        var eventPublisherMock = new Mock<IOrderEventPublisher>();

        inventoryOrderReservationClientMock
            .Setup(client => client.ReserveAsync(request.OrderId, request.CustomerId, It.IsAny<IReadOnlyCollection<ProductAvailabilityCheckItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductAvailabilityValidationResultDto
            {
                IsValid = true,
                Reason = "All items are available."
            });

        customerAddressClientMock
            .Setup(client => client.ValidateAsync(request.CustomerId, request.CustomerAddressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidatedCustomerAddressDto
            {
                CustomerId = request.CustomerId,
                AddressId = request.CustomerAddressId,
                CustomerEmail = "customer@example.com",
                FormattedAddress = "Rua A, 123"
            });

        readModelProjectorMock
            .Setup(projector => projector.ProjectAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("projection failed"));

        var provider = BuildProvider(
            writeDbContext,
            readModelProjectorMock.Object,
            customerAddressClientMock.Object,
            inventoryOrderReservationClientMock.Object,
            eventPublisherMock.Object);

        var service = CreateProcessorService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InvokeProcessMessageAsync(service, provider, outbox.Id));

        Assert.Equal("projection failed", exception.Message);

        var persistedOutbox = await writeDbContext.OrderProcessingOutboxMessages.SingleAsync(message => message.Id == outbox.Id);
        Assert.Equal(1, persistedOutbox.ProcessingAttempts);
        Assert.Equal("projection failed", persistedOutbox.LastProcessingError);
        Assert.Null(persistedOutbox.ProcessedAtUtc);

        eventPublisherMock.Verify(
            publisher => publisher.PublishOrderCreatedAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldIgnoreMessage_WhenPayloadDoesNotContainOutboxMessageId()
    {
        var writeDbContext = CreateWriteDbContext();
        var provider = BuildProvider(
            writeDbContext,
            Mock.Of<IOrderReadModelProjector>(),
            Mock.Of<ICustomerAddressValidationClient>(),
            Mock.Of<IInventoryOrderReservationClient>(),
            Mock.Of<IOrderEventPublisher>());

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, JsonSerializer.Serialize(new { Unknown = Guid.NewGuid() }));

        Assert.Empty(writeDbContext.Orders);
        Assert.Empty(writeDbContext.OrderProcessingOutboxMessages);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldIgnoreMessage_WhenOutboxMessageDoesNotExist()
    {
        var writeDbContext = CreateWriteDbContext();
        var provider = BuildProvider(
            writeDbContext,
            Mock.Of<IOrderReadModelProjector>(),
            Mock.Of<ICustomerAddressValidationClient>(),
            Mock.Of<IInventoryOrderReservationClient>(),
            Mock.Of<IOrderEventPublisher>());

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, Guid.NewGuid());

        Assert.Empty(writeDbContext.Orders);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldIgnoreMessage_WhenOutboxMessageIsAlreadyProcessed()
    {
        var writeDbContext = CreateWriteDbContext();
        var request = CreateOrderProcessingRequest();
        var outbox = await SeedOutboxAsync(writeDbContext, request);
        outbox.MarkAsProcessed();
        await writeDbContext.SaveChangesAsync();

        var inventoryOrderReservationClientMock = new Mock<IInventoryOrderReservationClient>();
        var provider = BuildProvider(
            writeDbContext,
            Mock.Of<IOrderReadModelProjector>(),
            Mock.Of<ICustomerAddressValidationClient>(),
            inventoryOrderReservationClientMock.Object,
            Mock.Of<IOrderEventPublisher>());

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, outbox.Id);

        Assert.Empty(writeDbContext.Orders);
        inventoryOrderReservationClientMock.Verify(
            client => client.ReserveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<ProductAvailabilityCheckItemDto>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldNotDuplicateRejectedOrder_WhenRejectedOrderAlreadyExists()
    {
        var writeDbContext = CreateWriteDbContext();
        var request = CreateOrderProcessingRequest();
        var existingRejectedOrder = Order.Domain.Entities.Order.CreateRejected(
            request.OrderId,
            request.CustomerId,
            request.CustomerAddressId,
            request.ShippingAmount,
            request.PaymentMethod,
            request.PaymentToken,
            request.PaymentCardBrand,
            request.PaymentCardLast4,
            request.Items.Select(item => new OrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity)),
            request.RequestedAtUtc,
            OrderRejectionReason.InsufficientStock,
            "Pedido rejeitado anteriormente.");
        await writeDbContext.Orders.AddAsync(existingRejectedOrder);
        var outbox = await SeedOutboxAsync(writeDbContext, request);

        var inventoryOrderReservationClientMock = new Mock<IInventoryOrderReservationClient>();
        inventoryOrderReservationClientMock
            .Setup(client => client.ReserveAsync(request.OrderId, request.CustomerId, It.IsAny<IReadOnlyCollection<ProductAvailabilityCheckItemDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductAvailabilityValidationResultDto
            {
                IsValid = false,
                Reason = "Insufficient stock.",
                Issues =
                [
                    new ProductAvailabilityIssueDto
                    {
                        ProductId = request.Items.First().ProductId,
                        ProductName = request.Items.First().ProductName,
                        RequestedQuantity = request.Items.First().Quantity,
                        AvailableQuantity = 0,
                        Reason = "Insufficient stock."
                    }
                ]
            });

        var provider = BuildProvider(
            writeDbContext,
            Mock.Of<IOrderReadModelProjector>(),
            Mock.Of<ICustomerAddressValidationClient>(),
            inventoryOrderReservationClientMock.Object,
            Mock.Of<IOrderEventPublisher>());

        var service = CreateProcessorService();

        await InvokeProcessMessageAsync(service, provider, outbox.Id);

        Assert.Equal(1, await writeDbContext.Orders.CountAsync());
    }

    private static OrderProcessorConsumerService CreateProcessorService()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        return new OrderProcessorConsumerService(
            Mock.Of<IServiceScopeFactory>(),
            configuration,
            NullLogger<OrderProcessorConsumerService>.Instance);
    }

    private static OrderWriteDbContext CreateWriteDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderWriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OrderWriteDbContext(options);
    }

    private static async Task<OrderProcessingOutboxMessage> SeedOutboxAsync(OrderWriteDbContext writeDbContext, OrderProcessingRequestDto request)
    {
        var outbox = OrderProcessingOutboxMessage.Create(
            request.OrderId,
            "order.processing.requested",
            nameof(OrderProcessingRequestDto),
            JsonSerializer.Serialize(request),
            request.RequestedAtUtc);

        await writeDbContext.OrderProcessingOutboxMessages.AddAsync(outbox);
        await writeDbContext.SaveChangesAsync();
        return outbox;
    }

    private static ServiceProvider BuildProvider(
        OrderWriteDbContext writeDbContext,
        IOrderReadModelProjector readModelProjector,
        ICustomerAddressValidationClient customerAddressValidationClient,
        IInventoryOrderReservationClient inventoryOrderReservationClient,
        IOrderEventPublisher eventPublisher)
    {
        var services = new ServiceCollection();
        services.AddSingleton(writeDbContext);
        services.AddSingleton(readModelProjector);
        services.AddSingleton(customerAddressValidationClient);
        services.AddSingleton(inventoryOrderReservationClient);
        services.AddSingleton(eventPublisher);
        return services.BuildServiceProvider();
    }

    private static async Task InvokeProcessMessageAsync(
        OrderProcessorConsumerService service,
        IServiceProvider serviceProvider,
        Guid outboxMessageId)
    {
        var payload = JsonSerializer.Serialize(new { OutboxMessageId = outboxMessageId });
        await InvokeProcessMessageAsync(service, serviceProvider, payload);
    }

    private static async Task InvokeProcessMessageAsync(
        OrderProcessorConsumerService service,
        IServiceProvider serviceProvider,
        string payload)
    {
        var method = typeof(OrderProcessorConsumerService).GetMethod(
            "ProcessMessageAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);

        var task = (Task?)method!.Invoke(service, [serviceProvider, payload, CancellationToken.None]);

        Assert.NotNull(task);
        await task!;
    }

    private static OrderProcessingRequestDto CreateOrderProcessingRequest()
    {
        return new OrderProcessingRequestDto
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerAddressId = Guid.NewGuid(),
            ShippingAmount = 20m,
            PaymentMethod = PaymentMethod.Credit,
            PaymentToken = "tok_123",
            PaymentCardBrand = "Visa",
            PaymentCardLast4 = "1234",
            RequestedAtUtc = DateTime.UtcNow,
            Items =
            [
                new OrderProcessingItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Produto",
                    UnitPrice = 100m,
                    Quantity = 2
                }
            ]
        };
    }

    private static Order.Domain.Entities.Order CreateExistingOrder(OrderProcessingRequestDto request)
    {
        return new Order.Domain.Entities.Order(
            request.OrderId,
            request.CustomerId,
            request.CustomerAddressId,
            "customer@example.com",
            "Rua A, 123",
            request.ShippingAmount,
            request.PaymentMethod,
            request.PaymentToken,
            request.PaymentCardBrand,
            request.PaymentCardLast4,
            request.Items.Select(item => new OrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity)),
            request.RequestedAtUtc);
    }
}
