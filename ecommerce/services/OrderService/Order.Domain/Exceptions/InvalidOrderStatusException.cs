using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class InvalidOrderStatusException : OrderException
{
    public InvalidOrderStatusException(string message)
        : base(OrderErrorCode.InvalidRequest, message)
    {
    }
}
