using Moq;
using Inventory.Application.Handlers;
using Inventory.Application.Interfaces;
using Inventory.Application.Queries;
using Inventory.Tests.Support;

namespace Inventory.Tests.Handlers;

public class GetInventoryAvailabilityHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAvailability_WhenRepositoryFindsItem()
    {
        var repository = new Mock<IInventoryRepository>();
        var productId = Guid.NewGuid();
        var availability = InventoryTestData.CreateAvailabilityDto(productId);
        repository.Setup(item => item.GetAvailabilityAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);

        var handler = new GetInventoryAvailabilityHandler(repository.Object);

        var result = await handler.Handle(new GetInventoryAvailabilityQuery(productId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(productId, result!.ProductId);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRepositoryFindsNothing()
    {
        var repository = new Mock<IInventoryRepository>();
        var handler = new GetInventoryAvailabilityHandler(repository.Object);

        var result = await handler.Handle(new GetInventoryAvailabilityQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
