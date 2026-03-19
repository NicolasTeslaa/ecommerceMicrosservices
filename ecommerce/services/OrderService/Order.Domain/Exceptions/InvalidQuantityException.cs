using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidQuantityException : OrderException
{
    public InvalidQuantityException() : base(OrderErrorCode.InvalidQuantity, "Quantity must be greater than zero.")
    {
    }
}
