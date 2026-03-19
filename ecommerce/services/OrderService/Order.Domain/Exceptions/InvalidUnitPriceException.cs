using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidUnitPriceException : OrderException
{
    public InvalidUnitPriceException() : base(OrderErrorCode.InvalidUnitPrice, "Unit price must be greater than zero.")
    {
    }
}
