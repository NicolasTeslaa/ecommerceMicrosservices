using Auth.Domain.Enums;

namespace Auth.Domain.Exceptions;

public class PersistenceException : AuthException
{
    public PersistenceException(string message, Exception? innerException = null)
        : base(AuthErrorCode.PersistenceFailure, message, innerException)
    {
    }
}
