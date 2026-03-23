using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public class InvalidCurrencyException : PaymentException
{
    public InvalidCurrencyException()
        : base(PaymentErrorCode.InvalidCurrency, "Payment currency is invalid.")
    {
    }
}
