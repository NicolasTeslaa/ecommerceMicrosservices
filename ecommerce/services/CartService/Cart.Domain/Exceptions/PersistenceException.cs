using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class PersistenceException : CartException
{
    public PersistenceException(string message, Exception? innerException = null)
        : base(CartErrorCode.PersistenceFailure, message, innerException)
    {
    }
}
