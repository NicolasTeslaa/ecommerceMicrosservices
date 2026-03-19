using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public abstract class OrderException : Exception
{
    protected OrderException(OrderErrorCode errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public OrderErrorCode ErrorCode { get; }
}
