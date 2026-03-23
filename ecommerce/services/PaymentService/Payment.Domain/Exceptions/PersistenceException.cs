using Payment.Domain.Enums;

namespace Payment.Domain.Exceptions;

public class PersistenceException : PaymentException
{
    public PersistenceException(string message, Exception? innerException = null)
        : base(PaymentErrorCode.PersistenceError, message)
    {
        if (innerException is not null)
        {
            Data["InnerException"] = innerException;
        }
    }
}
