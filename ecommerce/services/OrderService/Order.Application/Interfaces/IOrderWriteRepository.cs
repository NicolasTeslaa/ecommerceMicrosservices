namespace Order.Application.Interfaces;

public interface IOrderWriteRepository
{
    Task AddAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default);
}
