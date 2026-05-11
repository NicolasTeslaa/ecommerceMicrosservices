using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Auth.Infrastructure.Persistence;

public class AuthUserRepository : IAuthUserRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<AuthUserRepository> _logger;

    public AuthUserRepository(AuthDbContext context, ILogger<AuthUserRepository>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<AuthUserRepository>.Instance;
    }

    public async Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve auth user by email '{Email}'.", email);
            return null;
        }
    }

    public async Task AddAsync(AuthUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to persist auth user '{Email}'.", user.Email);
        }
    }
}
