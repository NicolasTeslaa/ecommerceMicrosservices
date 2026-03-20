using Auth.Application.Handlers;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Tests.Support;
using Moq;

namespace Auth.Tests.Handlers;

public class RegisterUserHandlerTests
{
    private readonly Mock<IAuthUserRepository> _repositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock = new();
    private readonly Mock<IAuthRegistrationService> _registrationServiceMock = new();
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _handler = new RegisterUserHandler(
            _repositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object,
            _registrationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUserAndReturnAuthResponse_WhenEmailIsAvailable()
    {
        var command = AuthTestData.CreateRegisterCommand();
        var expiration = DateTime.UtcNow.AddHours(1);

        _repositoryMock.Setup(repository => repository.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((AuthUser?)null);
        _passwordHasherMock.Setup(hasher => hasher.Hash(command.Password)).Returns("hashed");
        _tokenGeneratorMock.Setup(generator => generator.Generate(It.IsAny<AuthUser>())).Returns(("token-123", expiration));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal("Jane Doe", result.FullName);
        Assert.Equal("token-123", result.AccessToken);
        _registrationServiceMock.Verify(service => service.RegisterAsync(It.IsAny<AuthUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeEmailBeforeCheckingExistingUser()
    {
        var command = AuthTestData.CreateRegisterCommand(email: " Jane@Example.Com ");
        _repositoryMock.Setup(repository => repository.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((AuthUser?)null);
        _passwordHasherMock.Setup(hasher => hasher.Hash(command.Password)).Returns("hashed");
        _tokenGeneratorMock.Setup(generator => generator.Generate(It.IsAny<AuthUser>())).Returns(("token-123", DateTime.UtcNow.AddHours(1)));

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(repository => repository.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUserAlreadyExistsException_WhenEmailIsAlreadyRegistered()
    {
        var command = AuthTestData.CreateRegisterCommand();
        _repositoryMock.Setup(repository => repository.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(AuthTestData.CreateUser());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<UserAlreadyExistsException>(act);
    }

    [Fact]
    public async Task Handle_ShouldHashPasswordBeforeCreatingUser()
    {
        var command = AuthTestData.CreateRegisterCommand(password: "abc123");
        _repositoryMock.Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((AuthUser?)null);
        _passwordHasherMock.Setup(hasher => hasher.Hash("abc123")).Returns("hashed");
        _tokenGeneratorMock.Setup(generator => generator.Generate(It.IsAny<AuthUser>())).Returns(("token-123", DateTime.UtcNow.AddHours(1)));

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasherMock.Verify(hasher => hasher.Hash("abc123"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotRegisterUser_WhenEmailAlreadyExists()
    {
        var command = AuthTestData.CreateRegisterCommand();
        _repositoryMock.Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(AuthTestData.CreateUser());

        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _handler.Handle(command, CancellationToken.None));

        _registrationServiceMock.Verify(service => service.RegisterAsync(It.IsAny<AuthUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPropagateDomainValidation_WhenUserDataIsInvalid()
    {
        var command = AuthTestData.CreateRegisterCommand(fullName: "");
        _repositoryMock.Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((AuthUser?)null);
        _passwordHasherMock.Setup(hasher => hasher.Hash(command.Password)).Returns("hashed");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidFullNameException>(act);
    }
}
