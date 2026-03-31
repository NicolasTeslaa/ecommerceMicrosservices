using Moq;
using Inventory.Application.Handlers;
using Inventory.Application.Interfaces;
using Inventory.Application.Queries;
using Inventory.Tests.Support;

namespace Inventory.Tests.Handlers;

public class GetInventoryAvailabilityBatchHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAvailabilities_FromRepository()
    {
        var repository = new Mock<IInventoryRepository>();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var availability = new[]
        {
            InventoryTestData.CreateAvailabilityDto(productIds[0]),
            InventoryTestData.CreateAvailabilityDto(productIds[1])
        };
        repository.Setup(item => item.GetAvailabilityAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(productIds)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);

        var handler = new GetInventoryAvailabilityBatchHandler(repository.Object);

        var result = await handler.Handle(new GetInventoryAvailabilityBatchQuery(productIds), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyCollection_WhenRepositoryReturnsEmpty()
    {
        var repository = new Mock<IInventoryRepository>();
        repository.Setup(item => item.GetAvailabilityAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Inventory.Application.DTOs.InventoryAvailabilityDto>());

        var handler = new GetInventoryAvailabilityBatchHandler(repository.Object);

        var result = await handler.Handle(new GetInventoryAvailabilityBatchQuery(Array.Empty<Guid>()), CancellationToken.None);

        Assert.Empty(result);
    }
}
