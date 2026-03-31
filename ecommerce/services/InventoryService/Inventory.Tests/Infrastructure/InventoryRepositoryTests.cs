using Microsoft.EntityFrameworkCore;
using Inventory.Infrastructure.Persistence;
using Inventory.Tests.Support;

namespace Inventory.Tests.Infrastructure;

public class InventoryRepositoryTests
{
    [Fact]
    public async Task GetItemByProductIdAsync_ShouldReturnItem_WhenItExists()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var item = InventoryTestData.CreateItem();
        await context.InventoryItems.AddAsync(item);
        await context.SaveChangesAsync();

        var result = await repository.GetItemByProductIdAsync(item.ProductId);

        Assert.NotNull(result);
        Assert.Equal(item.ProductId, result!.ProductId);
    }

    [Fact]
    public async Task GetItemByProductIdAsync_ShouldReturnNull_WhenItDoesNotExist()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);

        var result = await repository.GetItemByProductIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemsByProductIdsAsync_ShouldReturnEmpty_WhenIdsCollectionIsEmpty()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);

        var result = await repository.GetItemsByProductIdsAsync(Array.Empty<Guid>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetItemsByProductIdsAsync_ShouldReturnOnlyMatchingItems()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var item1 = InventoryTestData.CreateItem();
        var item2 = InventoryTestData.CreateItem();
        var item3 = InventoryTestData.CreateItem();
        await context.InventoryItems.AddRangeAsync(item1, item2, item3);
        await context.SaveChangesAsync();

        var result = await repository.GetItemsByProductIdsAsync(new[] { item1.ProductId, item3.ProductId });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAvailabilityAsync_ShouldProjectAvailability_WhenItemExists()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var item = InventoryTestData.CreateItem(initialStockQuantity: 7);
        item.Reserve(2);
        await context.InventoryItems.AddAsync(item);
        await context.SaveChangesAsync();

        var result = await repository.GetAvailabilityAsync(item.ProductId);

        Assert.NotNull(result);
        Assert.Equal(5, result!.AvailableQuantity);
        Assert.Equal(2, result.ReservedQuantity);
    }

    [Fact]
    public async Task GetAvailabilityAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);

        var result = await repository.GetAvailabilityAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAvailabilityAsync_Batch_ShouldReturnEmpty_WhenIdsCollectionIsEmpty()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);

        var result = await repository.GetAvailabilityAsync(Array.Empty<Guid>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAvailabilityAsync_Batch_ShouldReturnProjectedItems()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var item1 = InventoryTestData.CreateItem(initialStockQuantity: 4);
        var item2 = InventoryTestData.CreateItem(initialStockQuantity: 6);
        await context.InventoryItems.AddRangeAsync(item1, item2);
        await context.SaveChangesAsync();

        var result = await repository.GetAvailabilityAsync(new[] { item1.ProductId, item2.ProductId });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetReservationsByOrderIdAsync_ShouldReturnReservations_ForOrder()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var orderId = Guid.NewGuid();
        await context.InventoryReservations.AddRangeAsync(
            InventoryTestData.CreateReservation(orderId: orderId),
            InventoryTestData.CreateReservation(orderId: orderId),
            InventoryTestData.CreateReservation(orderId: Guid.NewGuid()));
        await context.SaveChangesAsync();

        var result = await repository.GetReservationsByOrderIdAsync(orderId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AddItemAsync_AndSaveChangesAsync_ShouldPersistItem()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);

        await repository.AddItemAsync(InventoryTestData.CreateItem());
        await repository.SaveChangesAsync();

        Assert.Equal(1, await context.InventoryItems.CountAsync());
    }

    [Fact]
    public async Task AddReservationsAsync_AndSaveChangesAsync_ShouldPersistReservations()
    {
        await using var context = CreateDbContext();
        var repository = new InventoryRepository(context);
        var reservations = new[]
        {
            InventoryTestData.CreateReservation(),
            InventoryTestData.CreateReservation()
        };

        await repository.AddReservationsAsync(reservations);
        await repository.SaveChangesAsync();

        Assert.Equal(2, await context.InventoryReservations.CountAsync());
    }

    private static InventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InventoryDbContext(options);
    }
}
