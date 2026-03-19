using System.Net.Mail;
using Auth.Domain.Exceptions;

namespace Auth.Domain.Entities;

public class AuthUser
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool Active { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AuthUser()
    {
    }

    public AuthUser(string fullName, string email, string passwordHash)
    {
        Validate(fullName, email, passwordHash);

        Id = Guid.NewGuid();
        CustomerId = Guid.NewGuid();
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Active = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private static void Validate(string fullName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidFullNameException();

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException();

        try
        {
            _ = new MailAddress(email);
        }
        catch (FormatException)
        {
            throw new InvalidEmailException();
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidPasswordException();
    }
}
