using System.Diagnostics;
using System.Net.Mail;

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
        FullName = (fullName ?? string.Empty).Trim();
        Email = (email ?? string.Empty).Trim().ToLowerInvariant();
        PasswordHash = passwordHash ?? string.Empty;
        Active = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private static void Validate(string fullName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            Trace.TraceError("Invalid full name while creating auth user.");

        if (string.IsNullOrWhiteSpace(email))
            Trace.TraceError("Invalid email while creating auth user.");

        try
        {
            _ = new MailAddress(email ?? string.Empty);
        }
        catch (FormatException)
        {
            Trace.TraceError("Invalid email format while creating auth user.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
            Trace.TraceError("Invalid password hash while creating auth user.");
    }
}
