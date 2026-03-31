using ECommerce.Shared.Messaging;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence;
using Inventory.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Tests.Infrastructure;

public class PaymentFailedMessageProcessorTests
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
        await context.ProcessedKafkaMessages.AddAsync(new ProcessedKafkaMessage("payment.failed", 0, 20, "inventory-payment-failed"));
        await context.SaveChangesAsync();
        var processor = new PaymentFailedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId, true),
            "payment.failed",
            0,
            20,
            "inventory-payment-failed");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Pending, reservation.Status);
        Assert.Equal(3, item.ReservedQuantity);
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ShouldReleasePendingReservations_WhenMaxAttemptsReached()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 5);
        item.Reserve(2);
        var reservation = new InventoryReservation(orderId, productId, 2);
        await context.InventoryItems.AddAsync(item);
        await context.InventoryReservations.AddAsync(reservation);
        await context.SaveChangesAsync();
        var processor = new PaymentFailedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId, true),
            "payment.failed",
            1,
            21,
            "inventory-payment-failed");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
        Assert.Equal(5, item.AvailableQuantity);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotReleaseReservations_WhenMaxAttemptsWasNotReached()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 5);
        item.Reserve(2);
        var reservation = new InventoryReservation(orderId, productId, 2);
        await context.InventoryItems.AddAsync(item);
        await context.InventoryReservations.AddAsync(reservation);
        await context.SaveChangesAsync();
        var processor = new PaymentFailedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId, false),
            "payment.failed",
            2,
            22,
            "inventory-payment-failed");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Pending, reservation.Status);
        Assert.Equal(3, item.AvailableQuantity);
        Assert.Equal(2, item.ReservedQuantity);
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
        var processor = new PaymentFailedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId, true),
            "payment.failed",
            3,
            23,
            "inventory-payment-failed");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Pending, reservation.Status);
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ShouldLeaveConfirmedReservationUnchanged_WhenMaxAttemptsReached()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item = InventoryTestData.CreateItem(productId: productId, initialStockQuantity: 9);
        item.Reserve(4);
        item.ConfirmReservation(4);
        var reservation = new InventoryReservation(orderId, productId, 4);
        reservation.Confirm();
        await context.InventoryItems.AddAsync(item);
        await context.InventoryReservations.AddAsync(reservation);
        await context.SaveChangesAsync();
        var processor = new PaymentFailedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(orderId, true),
            "payment.failed",
            4,
            24,
            "inventory-payment-failed");

        Assert.True(result);
        Assert.Equal(InventoryReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(5, item.AvailableQuantity);
        Assert.Equal(0, item.ReservedQuantity);
    }

    [Fact]
    public async Task ProcessAsync_ShouldHandleOrderWithoutReservations()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var processor = new PaymentFailedMessageProcessor(context, repository);

        var result = await processor.ProcessAsync(
            CreateEvent(Guid.NewGuid(), true),
            "payment.failed",
            5,
            25,
            "inventory-payment-failed");

        Assert.True(result);
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
    }

    private static PaymentFailedIntegrationEvent CreateEvent(Guid orderId, bool maxAttemptsReached)
    {
        return new PaymentFailedIntegrationEvent
        {
            PaymentId = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = Guid.NewGuid(),
            Amount = 100,
            Currency = "BRL",
            FailureReason = "card_declined",
            FailureDetail = "declined",
            AttemptCount = maxAttemptsReached ? 3 : 1,
            MaxAttemptsReached = maxAttemptsReached,
            FailedAtUtc = DateTime.UtcNow
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
