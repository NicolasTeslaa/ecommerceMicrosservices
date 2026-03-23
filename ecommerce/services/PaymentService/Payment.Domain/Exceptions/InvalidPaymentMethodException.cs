using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public class InvalidPaymentMethodException : PaymentException
{
    public InvalidPaymentMethodException()
        : base(PaymentErrorCode.InvalidPaymentMethod, "Payment method is invalid.")
    {
    }
}
