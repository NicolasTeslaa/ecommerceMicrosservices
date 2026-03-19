using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidCustomerAddressIdException : OrderException
{
    public InvalidCustomerAddressIdException()
        : base(OrderErrorCode.InvalidCustomerAddressId, "Customer address id must be a valid non-empty value.")
    {
    }
}
