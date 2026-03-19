using Auth.Domain.Enums;

namespace Auth.Domain.Exceptions;

public abstract class AuthException : Exception
{
    protected AuthException(AuthErrorCode errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public AuthErrorCode ErrorCode { get; }
}
