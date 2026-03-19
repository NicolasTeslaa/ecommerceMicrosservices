using Auth.Domain.Enums;

namespace Auth.Domain.Exceptions;

public class InvalidCredentialsException : AuthException
{
    public InvalidCredentialsException()
        : base(AuthErrorCode.InvalidCredentials, "Email or password is invalid.")
    {
    }
}
