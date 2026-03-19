using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidPaymentMethodException : OrderException
{
    public InvalidPaymentMethodException() : base(OrderErrorCode.InvalidPaymentMethod, "Payment method is required.")
    {
    }
}
