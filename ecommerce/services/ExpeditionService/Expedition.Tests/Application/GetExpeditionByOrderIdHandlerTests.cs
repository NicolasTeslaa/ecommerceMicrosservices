using Expedition.Application.DTOs;
using Expedition.Application.Handlers;
using Expedition.Application.Interfaces;
using Expedition.Application.Queries;
using Expedition.Domain.Entities;
using Xunit;

namespace Expedition.Tests.Application;

public class GetExpeditionByOrderIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnExpedition_WhenItExists()
    {
        var expedition = new ExpeditionDto
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Status = "AwaitingCarrierPickup"
        };

        var handler = new GetExpeditionByOrderIdHandler(new FakeExpeditionRepository(expedition));

        var result = await handler.Handle(new GetExpeditionByOrderIdQuery(expedition.OrderId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expedition.OrderId, result!.OrderId);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenItDoesNotExist()
    {
        var handler = new GetExpeditionByOrderIdHandler(new FakeExpeditionRepository(null));

        var result = await handler.Handle(new GetExpeditionByOrderIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    private sealed class FakeExpeditionRepository : IExpeditionRepository
    {
        private readonly ExpeditionDto? _dto;

        public FakeExpeditionRepository(ExpeditionDto? dto)
        {
            _dto = dto;
        }

        public Task<ExpeditionDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dto);
        }

        public Task<ExpeditionOrder?> GetEntityByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ExpeditionOrder?>(null);
        }

        public Task AddAsync(ExpeditionOrder expeditionOrder, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
