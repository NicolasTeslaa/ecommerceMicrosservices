using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidCustomerEmailException : OrderException
{
    public InvalidCustomerEmailException() : base(OrderErrorCode.InvalidCustomerEmail, "Customer email is required and must be valid.")
    {
    }
}
