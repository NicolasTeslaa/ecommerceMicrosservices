using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidProductNameException : OrderException
{
    public InvalidProductNameException() : base(OrderErrorCode.InvalidProductName, "Product name is required.")
    {
    }
}
