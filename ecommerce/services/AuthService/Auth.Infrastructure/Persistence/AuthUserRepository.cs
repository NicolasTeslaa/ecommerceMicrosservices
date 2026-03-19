using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public class AuthUserRepository : IAuthUserRepository
{
    private readonly AuthDbContext _context;

    public AuthUserRepository(AuthDbContext context) => _context = context;

    public async Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to retrieve auth user by email '{email}'.", exception);
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
            throw new PersistenceException($"Failed to persist auth user '{user.Email}'.", exception);
        }
    }
}
