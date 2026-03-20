using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidPaymentCardDataException : OrderException
{
    public InvalidPaymentCardDataException()
        : base(OrderErrorCode.InvalidPaymentCardData, "Masked card metadata is invalid for this payment method.")
    {
    }
}
