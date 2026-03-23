using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public class InvalidOrderIdException : PaymentException
{
    public InvalidOrderIdException()
        : base(PaymentErrorCode.InvalidOrderId, "OrderId is invalid.")
    {
    }
}
