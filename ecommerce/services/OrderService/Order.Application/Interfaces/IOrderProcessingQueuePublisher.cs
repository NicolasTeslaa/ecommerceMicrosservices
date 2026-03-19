namespace Order.Application.Interfaces;

public interface IOrderProcessingQueuePublisher
{
    Task<bool> TryPublishAsync(Guid outboxMessageId, CancellationToken cancellationToken = default);
}
