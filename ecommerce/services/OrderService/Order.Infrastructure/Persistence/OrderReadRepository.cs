using Microsoft.EntityFrameworkCore;
using Order.Application.Interfaces;
using Order.Application.ReadModels;

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

    public async Task<IReadOnlyCollection<OrderReadModel>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
