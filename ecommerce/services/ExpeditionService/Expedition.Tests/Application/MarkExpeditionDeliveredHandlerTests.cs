using Expedition.Application.Commands;
using Expedition.Application.DTOs;
using Expedition.Application.Handlers;
using Expedition.Application.Interfaces;
using Expedition.Domain.Entities;
using Xunit;

namespace Expedition.Tests.Application;

public class MarkExpeditionDeliveredHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAdvanceStatus_AndPublishEvent()
    {
        var repository = new FakeExpeditionRepository();
        var publisher = new FakeExpeditionEventPublisher();
        var expedition = CreateExpedition();
        expedition.MarkAsPickedUp();
        expedition.MarkAsInTransit();
        repository.Expedition = expedition;
        var handler = new MarkExpeditionDeliveredHandler(repository, publisher);

        var result = await handler.Handle(new MarkExpeditionDeliveredCommand(expedition.OrderId), CancellationToken.None);

        Assert.Equal("Delivered", result.Status);
        Assert.Equal(1, repository.SaveChangesCalls);
        Assert.Single(publisher.PublishedStatuses);
        Assert.Equal("Delivered", publisher.PublishedStatuses.Single());
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderDoesNotExist()
    {
        var handler = new MarkExpeditionDeliveredHandler(new FakeExpeditionRepository(), new FakeExpeditionEventPublisher());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new MarkExpeditionDeliveredCommand(Guid.NewGuid()), CancellationToken.None));
    }

    private static ExpeditionOrder CreateExpedition()
    {
        return new ExpeditionOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            123,
            "1",
            "ACCESS-KEY",
            DateTime.UtcNow);
    }

    private sealed class FakeExpeditionRepository : IExpeditionRepository
    {
        public ExpeditionOrder? Expedition { get; set; }
        public int SaveChangesCalls { get; private set; }

        public Task<ExpeditionDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Expedition is null
                ? null
                : new ExpeditionDto
                {
                    Id = Expedition.Id,
                    OrderId = Expedition.OrderId,
                    InvoiceId = Expedition.InvoiceId,
                    CustomerId = Expedition.CustomerId,
                    InvoiceNumber = Expedition.InvoiceNumber,
                    InvoiceSeries = Expedition.InvoiceSeries,
                    InvoiceAccessKey = Expedition.InvoiceAccessKey,
                    Status = Expedition.Status.ToString(),
                    FailureReason = Expedition.FailureReason.ToString(),
                    FailureDetails = Expedition.FailureDetails
                });
        }

        public Task<ExpeditionOrder?> GetEntityByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Expedition);
        }

        public Task AddAsync(ExpeditionOrder expeditionOrder, CancellationToken cancellationToken = default)
        {
            Expedition = expeditionOrder;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExpeditionEventPublisher : IExpeditionEventPublisher
    {
        public List<string> PublishedStatuses { get; } = [];

        public Task PublishStatusChangedAsync(ExpeditionOrder expeditionOrder, CancellationToken cancellationToken = default)
        {
            PublishedStatuses.Add(expeditionOrder.Status.ToString());
            return Task.CompletedTask;
        }
    }
}
