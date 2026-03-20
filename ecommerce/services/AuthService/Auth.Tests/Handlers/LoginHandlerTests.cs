using Auth.Application.DTOs;
using Auth.Application.Handlers;
using Auth.Application.Interfaces;
using Auth.Domain.Exceptions;
using Auth.Tests.Support;
using Moq;

namespace Auth.Tests.Handlers;

public class LoginHandlerTests
{
    private readonly Mock<IAuthUserRepository> _repositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(_repositoryMock.Object, _passwordHasherMock.Object, _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        var command = AuthTestData.CreateLoginCommand();
        var user = AuthTestData.CreateUser();
        var expiration = DateTime.UtcNow.AddHours(1);

        _repositoryMock.Setup(repository => repository.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.Verify(command.Password, user.PasswordHash)).Returns(true);
        _tokenGeneratorMock.Setup(generator => generator.Generate(user)).Returns(("token-123", expiration));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.CustomerId, result.CustomerId);
        Assert.Equal("token-123", result.AccessToken);
        Assert.Equal(expiration, result.ExpiresAtUtc);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeEmailBeforeQueryingRepository()
    {
        var command = AuthTestData.CreateLoginCommand(email: " Jane@Example.Com ");
        var user = AuthTestData.CreateUser();

        _repositoryMock.Setup(repository => repository.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.Verify(command.Password, user.PasswordHash)).Returns(true);
        _tokenGeneratorMock.Setup(generator => generator.Generate(user)).Returns(("token-123", DateTime.UtcNow.AddHours(1)));

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(repository => repository.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCredentialsException_WhenUserDoesNotExist()
    {
        var command = AuthTestData.CreateLoginCommand();
        _repositoryMock.Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Auth.Domain.Entities.AuthUser?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCredentialsException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCredentialsException_WhenPasswordVerificationFails()
    {
        var command = AuthTestData.CreateLoginCommand();
        var user = AuthTestData.CreateUser();

        _repositoryMock.Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.Verify(command.Password, user.PasswordHash)).Returns(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCredentialsException>(act);
    }

    [Fact]
    public async Task Handle_ShouldNotGenerateToken_WhenCredentialsAreInvalid()
    {
        var command = AuthTestData.CreateLoginCommand();
        _repositoryMock.Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Auth.Domain.Entities.AuthUser?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _handler.Handle(command, CancellationToken.None));

        _tokenGeneratorMock.Verify(generator => generator.Generate(It.IsAny<Auth.Domain.Entities.AuthUser>()), Times.Never);
    }
}
