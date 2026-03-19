using Auth.Domain.Enums;

namespace Auth.Domain.Exceptions;

public class InvalidPasswordException : AuthException
{
    public InvalidPasswordException()
        : base(AuthErrorCode.InvalidPassword, "Password must have at least 6 characters.")
    {
    }
}
