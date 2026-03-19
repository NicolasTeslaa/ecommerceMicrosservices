using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class PersistenceException : OrderException
{
    public PersistenceException(string message)
        : base(OrderErrorCode.PersistenceError, message)
    {
    }
}
