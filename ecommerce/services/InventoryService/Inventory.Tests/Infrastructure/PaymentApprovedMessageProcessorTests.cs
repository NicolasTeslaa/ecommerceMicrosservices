using ECommerce.Shared.Messaging;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence;
using Inventory.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Tests.Infrastructure;

public class PaymentApprovedMessageProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldReturnTrueWithoutChanges_WhenMessageWasAlreadyProcessed()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 10);
        item.Reserve(3);
        var reservation = new InventoryReservation(orderId, productId, 3);
        await context.InventoryItems.AddAsync(item);
        await context.InventoryReservations.AddAsync(reservation);
        await context.ProcessedKafkaMessages.AddAsync(new ProcessedKafkaMessage("payment.approved", 0, 10, "inventory-payment-approved"));
        await context.SaveChangesAsync();
        var processor = new PaymentApprovedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId),
            "payment.approved",
            0,
            10,
            "inventory-payment-approved");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Pending, reservation.Status);
        Assert.Equal(3, item.ReservedQuantity);
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ShouldConfirmPendingReservations_WhenInventoryItemsExist()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 8);
        item.Reserve(3);
        var reservation = new InventoryReservation(orderId, productId, 3);
        await context.InventoryItems.AddAsync(item);
        await context.InventoryReservations.AddAsync(reservation);
        await context.SaveChangesAsync();
        var processor = new PaymentApprovedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId),
            "payment.approved",
            1,
            11,
            "inventory-payment-approved");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Equal(5, item.AvailableQuantity);
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ShouldIgnoreReservation_WhenInventoryItemDoesNotExist()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        var reservation = InventoryTestData.CreateReservation(orderId: orderId, quantity: 2);
        await context.InventoryReservations.AddAsync(reservation);
        await context.SaveChangesAsync();
        var processor = new PaymentApprovedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId),
            "payment.approved",
            2,
            12,
            "inventory-payment-approved");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Pending, reservation.Status);
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ShouldLeaveReleasedReservationUnchanged()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 10);
        item.Reserve(2);
        item.ReleaseReservation(2);
        var reservation = new InventoryReservation(orderId, productId, 2);
        reservation.Release();
        await context.InventoryItems.AddAsync(item);
        await context.InventoryReservations.AddAsync(reservation);
        await context.SaveChangesAsync();
        var processor = new PaymentApprovedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId),
            "payment.approved",
            3,
            13,
            "inventory-payment-approved");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Equal(10, item.AvailableQuantity);
    }

    [Fact]
    public async Task ProcessAsync_ShouldHandleOrderWithoutReservations()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var processor = new PaymentApprovedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(Guid.NewGuid()),
            "payment.approved",
            4,
            14,
            "inventory-payment-approved");

        Assert.True(result);
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ShouldConfirmMultiplePendingReservations_ForSameOrder()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        var firstItem = InventoryTestData.CreateItem(productId: firstProductId, initialStockQuantity: 9);
        var secondItem = InventoryTestData.CreateItem(productId: secondProductId, initialStockQuantity: 7);
        firstItem.Reserve(4);
        secondItem.Reserve(2);
        var firstReservation = new InventoryReservation(orderId, firstProductId, 4);
        var secondReservation = new InventoryReservation(orderId, secondProductId, 2);
        await context.InventoryItems.AddRangeAsync(firstItem, secondItem);
        await context.InventoryReservations.AddRangeAsync(firstReservation, secondReservation);
        await context.SaveChangesAsync();
        var processor = new PaymentApprovedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId),
            "payment.approved",
            5,
            15,
            "inventory-payment-approved");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Confirmed, firstReservation.Status);
        Assert.Equal(InventoryReservationStatus.Confirmed, secondReservation.Status);
        Assert.Equal(0, firstItem.ReservedQuantity);
        Assert.Equal(0, secondItem.ReservedQuantity);
    }

    private static PaymentApprovedIntegrationEvent CreateEvent(Guid orderId)
    {
        return new PaymentApprovedIntegrationEvent
        {
            PaymentId = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = Guid.NewGuid(),
            Amount = 100,
            Currency = "BRL",
            StripePaymentIntentId = "pi_test",
            ApprovedAtUtc = DateTime.UtcNow
        };
    }

    private static InventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InventoryDbContext(options);
    }
}
