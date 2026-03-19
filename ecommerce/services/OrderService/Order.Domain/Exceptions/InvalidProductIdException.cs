using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidProductIdException : OrderException
{
    public InvalidProductIdException() : base(OrderErrorCode.InvalidProductId, "Product id must be a valid non-empty value.")
    {
    }
}
