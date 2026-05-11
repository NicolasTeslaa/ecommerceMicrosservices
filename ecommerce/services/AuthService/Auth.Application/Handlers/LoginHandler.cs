using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using MediatR;

namespace Auth.Application.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public LoginHandler(
        IAuthUserRepository repository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _repository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return new AuthResponseDto { Email = email, FullName = string.Empty, AccessToken = string.Empty };

        var (accessToken, expiresAtUtc) = _tokenGenerator.Generate(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            CustomerId = user.CustomerId,
            FullName = user.FullName,
            Email = user.Email,
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
