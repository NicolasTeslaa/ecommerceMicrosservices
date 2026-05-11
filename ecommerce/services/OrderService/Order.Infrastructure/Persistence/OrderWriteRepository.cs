using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Persistence;

public class OrderWriteRepository : IOrderWriteRepository
{
    private readonly OrderWriteDbContext _dbContext;
    private readonly ILogger<OrderWriteRepository> _logger;

    public OrderWriteRepository(OrderWriteDbContext dbContext, ILogger<OrderWriteRepository>? logger = null)
    {
        _dbContext = dbContext;
        _logger = logger ?? NullLogger<OrderWriteRepository>.Instance;
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
            _logger.LogError(exception, "Failed to persist order '{OrderId}'.", order.Id);
        }
    }
}
