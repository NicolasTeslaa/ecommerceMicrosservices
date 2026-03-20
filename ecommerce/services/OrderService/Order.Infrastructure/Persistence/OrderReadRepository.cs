using ECommerce.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Order.Application.Interfaces;
using Order.Application.ReadModels;
using Order.Domain.Exceptions;

namespace Order.Infrastructure.Persistence;

public class OrderReadRepository : IOrderReadRepository
{
    private readonly OrderReadDbContext _dbContext;

    public OrderReadRepository(OrderReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderReadModel?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public async Task<PagedResult<OrderReadModel>> GetByCustomerIdAsync(
        Guid customerId,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.Orders
                .AsNoTracking()
                .Include(order => order.Items)
                .Where(order => order.CustomerId == customerId)
                .OrderByDescending(order => order.CreatedAtUtc)
                .ThenByDescending(order => order.Id);

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return PagedResult<OrderReadModel>.Create(items, pagination.PageNumber, pagination.PageSize, totalItems);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to retrieve orders for customer '{customerId}' from the read database. {exception}");
        }
    }
}
