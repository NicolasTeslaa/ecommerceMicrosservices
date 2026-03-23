using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public abstract class PaymentException : Exception
{
    protected PaymentException(PaymentErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public PaymentErrorCode ErrorCode { get; }
}
