using ECommerce.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Order.Application.Interfaces;
using Order.Application.ReadModels;

namespace Order.Infrastructure.Persistence;

public class OrderReadRepository : IOrderReadRepository
{
    private readonly OrderReadDbContext _dbContext;
    private readonly ILogger<OrderReadRepository> _logger;

    public OrderReadRepository(OrderReadDbContext dbContext, ILogger<OrderReadRepository>? logger = null)
    {
        _dbContext = dbContext;
        _logger = logger ?? NullLogger<OrderReadRepository>.Instance;
    }

    public async Task<OrderReadModel?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .Include(order => order.Items)
                .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve order '{OrderId}' from the read database.", orderId);
            return null;
        }
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
            _logger.LogError(exception, "Failed to retrieve orders for customer '{CustomerId}' from the read database.", customerId);
            return PagedResult<OrderReadModel>.Create(Array.Empty<OrderReadModel>(), pagination.PageNumber, pagination.PageSize, 0);
        }
    }
}
