using Inventory.Domain.Entities;
using Inventory.Tests.Support;

namespace Inventory.Tests.Domain;

public class InventoryItemTests
{
    [Fact]
    public void Constructor_ShouldCreateItem_WhenDataIsValid()
    {
        var productId = Guid.NewGuid();

        var item = new InventoryItem(productId, " Produto ", 8, true);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal("Produto", item.ProductName);
        Assert.Equal(8, item.AvailableQuantity);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.True(item.Active);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProductIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new InventoryItem(Guid.Empty, "Produto", 1, true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProductNameIsBlank()
    {
        Assert.Throws<ArgumentException>(() => new InventoryItem(Guid.NewGuid(), "   ", 1, true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenInitialStockIsNegative()
    {
        Assert.Throws<ArgumentException>(() => new InventoryItem(Guid.NewGuid(), "Produto", -1, true));
    }

    [Fact]
    public void CanReserve_ShouldReturnTrue_WhenItemIsActiveAndHasEnoughStock()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 5, active: true);

        var canReserve = item.CanReserve(4);

        Assert.True(canReserve);
    }

    [Fact]
    public void CanReserve_ShouldReturnFalse_WhenItemIsInactive()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 5, active: false);

        var canReserve = item.CanReserve(1);

        Assert.False(canReserve);
    }

    [Fact]
    public void CanReserve_ShouldReturnFalse_WhenQuantityIsZero()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 5, active: true);

        var canReserve = item.CanReserve(0);

        Assert.False(canReserve);
    }

    [Fact]
    public void CanReserve_ShouldReturnFalse_WhenStockIsInsufficient()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 2, active: true);

        var canReserve = item.CanReserve(3);

        Assert.False(canReserve);
    }

    [Fact]
    public void IncreaseStock_ShouldAddToAvailableQuantity()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 2);
        var previousUpdatedAt = item.UpdatedAtUtc;

        item.IncreaseStock(3);

        Assert.Equal(5, item.AvailableQuantity);
        Assert.True(item.UpdatedAtUtc >= previousUpdatedAt);
    }

    [Fact]
    public void IncreaseStock_ShouldThrow_WhenQuantityIsNotPositive()
    {
        var item = InventoryTestData.CreateItem();

        Assert.Throws<ArgumentException>(() => item.IncreaseStock(0));
    }

    [Fact]
    public void Reserve_ShouldMoveUnitsFromAvailableToReserved()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 7);

        item.Reserve(3);

        Assert.Equal(4, item.AvailableQuantity);
        Assert.Equal(3, item.ReservedQuantity);
    }

    [Fact]
    public void Reserve_ShouldThrow_WhenCannotReserve()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 1);

        Assert.Throws<InvalidOperationException>(() => item.Reserve(2));
    }

    [Fact]
    public void ConfirmReservation_ShouldDecreaseReservedQuantity()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 6);
        item.Reserve(4);

        item.ConfirmReservation(3);

        Assert.Equal(2, item.AvailableQuantity);
        Assert.Equal(1, item.ReservedQuantity);
    }

    [Fact]
    public void ConfirmReservation_ShouldThrow_WhenReservedQuantityIsInsufficient()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 5);
        item.Reserve(1);

        Assert.Throws<InvalidOperationException>(() => item.ConfirmReservation(2));
    }

    [Fact]
    public void ReleaseReservation_ShouldRestoreAvailableQuantity()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 10);
        item.Reserve(4);

        item.ReleaseReservation(3);

        Assert.Equal(9, item.AvailableQuantity);
        Assert.Equal(1, item.ReservedQuantity);
    }

    [Fact]
    public void ReleaseReservation_ShouldThrow_WhenReservedQuantityIsInsufficient()
    {
        var item = InventoryTestData.CreateItem(initialStockQuantity: 10);
        item.Reserve(2);

        Assert.Throws<InvalidOperationException>(() => item.ReleaseReservation(3));
    }

    [Fact]
    public void UpdateCatalogMetadata_ShouldUpdateTrimmedNameAndActiveFlag()
    {
        var item = InventoryTestData.CreateItem(productName: "Antigo", active: true);

        item.UpdateCatalogMetadata(" Novo nome ", false);

        Assert.Equal("Novo nome", item.ProductName);
        Assert.False(item.Active);
    }

    [Fact]
    public void UpdateCatalogMetadata_ShouldKeepExistingName_WhenNewNameIsBlank()
    {
        var item = InventoryTestData.CreateItem(productName: "Mantido", active: true);

        item.UpdateCatalogMetadata("   ", false);

        Assert.Equal("Mantido", item.ProductName);
        Assert.False(item.Active);
    }
}
