using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class CustomerNotFoundException : CustomerException
{
    public CustomerNotFoundException(Guid customerId)
        : base(CustomerErrorCode.CustomerNotFound, $"Customer '{customerId}' was not found.")
    {
    }
}
