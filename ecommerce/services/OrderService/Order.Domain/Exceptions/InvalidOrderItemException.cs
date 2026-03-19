using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidOrderItemException : OrderException
{
    public InvalidOrderItemException() : base(OrderErrorCode.InvalidOrderItem, "Order must contain at least one valid item.")
    {
    }
}
