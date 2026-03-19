namespace Order.Application.Interfaces;

public interface IOrderReadModelProjector
{
    Task ProjectAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default);
}
