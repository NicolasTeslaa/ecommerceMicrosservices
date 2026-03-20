using Auth.Application.Commands;
using Auth.Domain.Entities;

namespace Auth.Tests.Support;

internal static class AuthTestData
{
    public static AuthUser CreateUser(
        string fullName = "Jane Doe",
        string email = "jane@example.com",
        string passwordHash = "hashed-password")
    {
        return new AuthUser(fullName, email, passwordHash);
    }

    public static RegisterUserCommand CreateRegisterCommand(
        string fullName = "Jane Doe",
        string email = "jane@example.com",
        string password = "secret123")
    {
        return new RegisterUserCommand
        {
            FullName = fullName,
            Email = email,
            Password = password
        };
    }

    public static LoginCommand CreateLoginCommand(
        string email = "jane@example.com",
        string password = "secret123")
    {
        return new LoginCommand
        {
            Email = email,
            Password = password
        };
    }
}
