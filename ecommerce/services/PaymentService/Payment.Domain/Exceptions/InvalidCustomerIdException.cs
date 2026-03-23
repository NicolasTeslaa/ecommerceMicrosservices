using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public class InvalidCustomerIdException : PaymentException
{
    public InvalidCustomerIdException()
        : base(PaymentErrorCode.InvalidCustomerId, "CustomerId is invalid.")
    {
    }
}
