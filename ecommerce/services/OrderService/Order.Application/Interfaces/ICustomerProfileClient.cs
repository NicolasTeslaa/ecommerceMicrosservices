using Order.Application.DTOs;

namespace Order.Application.Interfaces;

public interface ICustomerProfileClient
{
    Task<CustomerProfileDto> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
