using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public class PaymentNotFoundException : PaymentException
{
    public PaymentNotFoundException(Guid orderId)
        : base(PaymentErrorCode.PaymentNotFound, $"Payment for order '{orderId}' was not found.")
    {
    }
}
