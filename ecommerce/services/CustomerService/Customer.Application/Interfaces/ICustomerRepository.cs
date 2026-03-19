using Customer.Domain.Entities;

namespace Customer.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer.Domain.Entities.Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Customer.Domain.Entities.Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer.Domain.Entities.Customer customer, CancellationToken cancellationToken = default);
}
