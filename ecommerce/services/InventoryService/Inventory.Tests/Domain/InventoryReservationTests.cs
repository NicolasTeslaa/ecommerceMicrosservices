using Inventory.Domain.Enums;
using Inventory.Tests.Support;

namespace Inventory.Tests.Domain;

public class InventoryReservationTests
{
    [Fact]
    public void Constructor_ShouldCreatePendingReservation_WhenDataIsValid()
    {
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var reservation = new Inventory.Domain.Entities.InventoryReservation(orderId, productId, 3);

        Assert.Equal(orderId, reservation.OrderId);
        Assert.Equal(productId, reservation.ProductId);
        Assert.Equal(3, reservation.Quantity);
        Assert.Equal(InventoryReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOrderIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Inventory.Domain.Entities.InventoryReservation(Guid.Empty, Guid.NewGuid(), 1));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProductIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Inventory.Domain.Entities.InventoryReservation(Guid.NewGuid(), Guid.Empty, 1));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsZero()
    {
        Assert.Throws<ArgumentException>(() => new Inventory.Domain.Entities.InventoryReservation(Guid.NewGuid(), Guid.NewGuid(), 0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsNegative()
    {
        Assert.Throws<ArgumentException>(() => new Inventory.Domain.Entities.InventoryReservation(Guid.NewGuid(), Guid.NewGuid(), -1));
    }

    [Fact]
    public void Confirm_ShouldSetStatusToConfirmed()
    {
        var reservation = InventoryTestData.CreateReservation();

        reservation.Confirm();

        Assert.Equal(InventoryReservationStatus.Confirmed, reservation.Status);
    }

    [Fact]
    public void Release_ShouldSetStatusToReleased()
    {
        var reservation = InventoryTestData.CreateReservation();

        reservation.Release();

        Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
    }
}
