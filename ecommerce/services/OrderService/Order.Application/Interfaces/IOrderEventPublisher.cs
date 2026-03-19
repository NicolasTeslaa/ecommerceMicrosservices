namespace Order.Application.Interfaces;

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default);
}
