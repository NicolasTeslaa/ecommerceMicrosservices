using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidPaymentTokenException : OrderException
{
    public InvalidPaymentTokenException()
        : base(OrderErrorCode.InvalidPaymentToken, "A payment token is required for card payments.")
    {
    }
}
