using Customer.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Customer.Infrastructure.Persistence;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(CustomerDbContext context, ILogger<CustomerRepository>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<CustomerRepository>.Instance;
    }

    public async Task<Customer.Domain.Entities.Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Customers
                .Include(customer => customer.Addresses)
                .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve customer '{CustomerId}'.", id);
            return null;
        }
    }

    public async Task AddAsync(Customer.Domain.Entities.Customer customer, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to persist customer '{CustomerId}'.", customer.Id);
        }
    }

    public async Task UpdateAsync(Domain.Entities.Customer customer, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var address in customer.Addresses)
            {
                var entry = _context.Entry(address);

                if (entry.State == EntityState.Detached)
                {
                    _context.Entry(address).State = EntityState.Added;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to update customer '{CustomerId}'.", customer.Id);
        }
    }
}
