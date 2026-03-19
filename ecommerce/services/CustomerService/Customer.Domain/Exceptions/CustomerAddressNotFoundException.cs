using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class CustomerAddressNotFoundException : CustomerException
{
    public CustomerAddressNotFoundException(Guid customerId, Guid addressId)
        : base(CustomerErrorCode.CustomerAddressNotFound, $"Address '{addressId}' was not found for customer '{customerId}'.")
    {
    }
}
