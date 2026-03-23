using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public class InvalidPaymentIntentException : PaymentException
{
    public InvalidPaymentIntentException()
        : base(PaymentErrorCode.InvalidPaymentIntent, "PaymentIntent data is invalid.")
    {
    }
}
