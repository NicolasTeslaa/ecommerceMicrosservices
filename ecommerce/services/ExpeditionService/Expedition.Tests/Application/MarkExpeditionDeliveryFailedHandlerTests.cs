using Expedition.Application.Commands;
using Expedition.Application.Handlers;
using Expedition.Application.Interfaces;
using Expedition.Domain.Entities;
using Xunit;

namespace Expedition.Tests.Application;

public class MarkExpeditionDeliveryFailedHandlerTests
{
    [Fact]
    public async Task Handle_ShouldThrow_WhenFailureReasonIsUnsupported()
    {
        var repository = new FakeExpeditionRepository();
        var expedition = CreateExpedition();
        expedition.MarkAsPickedUp();
        expedition.MarkAsInTransit();
        repository.Expedition = expedition;

        var handler = new MarkExpeditionDeliveryFailedHandler(repository, new FakeExpeditionEventPublisher());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new MarkExpeditionDeliveryFailedCommand(expedition.OrderId, "UnknownReason", "details"),
                CancellationToken.None));
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

        public Task<Expedition.Application.DTOs.ExpeditionDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Expedition is null
                ? null
                : new Expedition.Application.DTOs.ExpeditionDto
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
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExpeditionEventPublisher : IExpeditionEventPublisher
    {
        public Task PublishStatusChangedAsync(ExpeditionOrder expeditionOrder, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
