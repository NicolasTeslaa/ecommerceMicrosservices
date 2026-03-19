using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class CustomerAddressNotFoundException : OrderException
{
    public CustomerAddressNotFoundException(Guid customerId, Guid customerAddressId)
        : base(OrderErrorCode.CustomerAddressNotFound, $"Address '{customerAddressId}' was not found for customer '{customerId}'.")
    {
    }
}
