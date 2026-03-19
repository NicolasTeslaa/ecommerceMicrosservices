using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

public interface IAuthUserRepository
{
    Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(AuthUser user, CancellationToken cancellationToken = default);
}
