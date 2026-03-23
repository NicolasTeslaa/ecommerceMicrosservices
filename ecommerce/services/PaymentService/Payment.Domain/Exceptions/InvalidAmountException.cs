using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public class InvalidAmountException : PaymentException
{
    public InvalidAmountException()
        : base(PaymentErrorCode.InvalidAmount, "Payment amount must be greater than zero.")
    {
    }
}
