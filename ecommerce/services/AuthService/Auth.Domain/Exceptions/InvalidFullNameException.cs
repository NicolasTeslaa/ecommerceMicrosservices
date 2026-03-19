using Auth.Domain.Enums;

namespace Auth.Domain.Exceptions;

public class InvalidFullNameException : AuthException
{
    public InvalidFullNameException()
        : base(AuthErrorCode.InvalidFullName, "Full name must be provided.")
    {
    }
}
