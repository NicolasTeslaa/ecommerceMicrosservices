using Microsoft.EntityFrameworkCore;
using Order.Application.Interfaces;
using Order.Domain.Exceptions;

namespace Order.Infrastructure.Persistence;

public class OrderWriteRepository : IOrderWriteRepository
{
    private readonly OrderWriteDbContext _dbContext;

    public OrderWriteRepository(OrderWriteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Orders.AddAsync(order, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new PersistenceException($"Failed to persist order '{order.Id}'. {exception.Message}");
        }
    }
}
