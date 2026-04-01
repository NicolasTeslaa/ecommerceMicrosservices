using Expedition.Domain.Entities;
using Expedition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Expedition.Tests.Infrastructure;

public class ExpeditionRepositoryTests
{
    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnDto()
    {
        var options = new DbContextOptionsBuilder<ExpeditionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ExpeditionDbContext(options);
        var repository = new ExpeditionRepository(context);
        var expedition = new ExpeditionOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            789,
            "1",
            "ACCESS-KEY",
            DateTime.UtcNow);

        await repository.AddAsync(expedition);
        await repository.SaveChangesAsync();

        var dto = await repository.GetByOrderIdAsync(expedition.OrderId);

        Assert.NotNull(dto);
        Assert.Equal(expedition.OrderId, dto!.OrderId);
        Assert.Equal("AwaitingCarrierPickup", dto.Status);
    }
}
