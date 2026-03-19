using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

public interface ITokenGenerator
{
    (string AccessToken, DateTime ExpiresAtUtc) Generate(AuthUser user);
}
