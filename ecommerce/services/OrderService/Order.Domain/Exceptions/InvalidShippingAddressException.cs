using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidShippingAddressException : OrderException
{
    public InvalidShippingAddressException() : base(OrderErrorCode.InvalidShippingAddress, "Shipping address is required.")
    {
    }
}
