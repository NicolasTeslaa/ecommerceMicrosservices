namespace Notification.Application.Interfaces;

public interface ICustomerContactClient
{
    Task<CustomerContact?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
