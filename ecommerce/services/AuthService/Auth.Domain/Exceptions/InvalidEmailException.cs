using Auth.Domain.Enums;

namespace Auth.Domain.Exceptions;

public class InvalidEmailException : AuthException
{
    public InvalidEmailException()
        : base(AuthErrorCode.InvalidEmail, "A valid email address must be provided.")
    {
    }
}
