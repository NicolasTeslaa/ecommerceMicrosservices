using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

public interface IAuthRegistrationService
{
    Task RegisterAsync(AuthUser user, string phoneNumber, CancellationToken cancellationToken = default);
}
