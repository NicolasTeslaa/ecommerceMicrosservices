using ECommerce.Shared.Contracts;
using Order.Application.ReadModels;

namespace Order.Application.Interfaces;

public interface IOrderReadRepository
{
    Task<OrderReadModel?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderReadModel>> GetByCustomerIdAsync(Guid customerId, PaginationRequest pagination, CancellationToken cancellationToken = default);
}
