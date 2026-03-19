using Order.Application.ReadModels;

namespace Order.Application.Interfaces;

public interface IOrderReadRepository
{
    Task<OrderReadModel?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrderReadModel>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
