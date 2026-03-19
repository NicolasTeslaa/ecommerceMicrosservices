using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidCustomerIdException : OrderException
{
    public InvalidCustomerIdException() : base(OrderErrorCode.InvalidCustomerId, "Customer id must be a valid non-empty value.")
    {
    }
}
