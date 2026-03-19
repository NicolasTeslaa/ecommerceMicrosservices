using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class PersistenceException : CustomerException
{
    public PersistenceException(string message, Exception? innerException = null)
        : base(CustomerErrorCode.PersistenceFailure, message, innerException)
    {
    }
}
