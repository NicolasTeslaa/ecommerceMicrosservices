using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;

namespace Auth.Application.Handlers;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IAuthUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IAuthRegistrationService _registrationService;

    public RegisterUserHandler(
        IAuthUserRepository repository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        IAuthRegistrationService registrationService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _registrationService = registrationService;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _repository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
            return new AuthResponseDto
            {
                UserId = existingUser.Id,
                CustomerId = existingUser.CustomerId,
                FullName = existingUser.FullName,
                Email = existingUser.Email,
                AccessToken = string.Empty
            };

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new AuthUser(request.FullName, email, passwordHash);

        await _registrationService.RegisterAsync(user, request.PhoneNumber, cancellationToken);

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
