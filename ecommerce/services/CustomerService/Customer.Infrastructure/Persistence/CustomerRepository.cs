using Customer.Application.Interfaces;
using Customer.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure.Persistence;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context) => _context = context;

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
            throw new PersistenceException($"Failed to retrieve customer '{id}'.", exception);
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
            throw new PersistenceException($"Failed to persist customer '{customer.Id}'.", exception);
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
            throw new PersistenceException($"Failed to update customer '{customer.Id}'.", exception);
        }
    }
}
