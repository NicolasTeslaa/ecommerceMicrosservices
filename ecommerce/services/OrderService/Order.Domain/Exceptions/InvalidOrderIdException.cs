using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidOrderIdException : OrderException
{
    public InvalidOrderIdException() : base(OrderErrorCode.InvalidOrderId, "Order id must be a valid non-empty value.")
    {
    }
}
