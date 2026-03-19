using Auth.Domain.Enums;

namespace Auth.Domain.Exceptions;

public class UserAlreadyExistsException : AuthException
{
    public UserAlreadyExistsException(string email)
        : base(AuthErrorCode.UserAlreadyExists, $"User with email '{email}' already exists.")
    {
    }
}
