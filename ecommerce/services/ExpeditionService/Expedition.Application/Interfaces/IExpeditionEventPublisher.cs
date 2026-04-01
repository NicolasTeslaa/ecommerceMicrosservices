using Expedition.Domain.Entities;

namespace Expedition.Application.Interfaces;

public interface IExpeditionEventPublisher
{
    Task PublishStatusChangedAsync(ExpeditionOrder expeditionOrder, CancellationToken cancellationToken = default);
}
