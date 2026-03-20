using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Tests.Support;

namespace Auth.Tests.Domain;

public class AuthUserTests
{
    [Fact]
    public void Constructor_ShouldCreateUser_WhenDataIsValid()
    {
        var user = AuthTestData.CreateUser();

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.NotEqual(Guid.Empty, user.CustomerId);
        Assert.Equal("Jane Doe", user.FullName);
        Assert.Equal("jane@example.com", user.Email);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.True(user.Active);
    }

    [Fact]
    public void Constructor_ShouldTrimFullName_AndNormalizeEmail()
    {
        var user = AuthTestData.CreateUser(fullName: " Jane Doe ", email: " Jane@Example.Com ");

        Assert.Equal("Jane Doe", user.FullName);
        Assert.Equal("jane@example.com", user.Email);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidFullNameException_WhenFullNameIsEmpty()
    {
        var act = () => AuthTestData.CreateUser(fullName: "");

        Assert.Throws<InvalidFullNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidFullNameException_WhenFullNameIsWhitespace()
    {
        var act = () => AuthTestData.CreateUser(fullName: "   ");

        Assert.Throws<InvalidFullNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidEmailException_WhenEmailIsEmpty()
    {
        var act = () => AuthTestData.CreateUser(email: "");

        Assert.Throws<InvalidEmailException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidEmailException_WhenEmailIsInvalid()
    {
        var act = () => AuthTestData.CreateUser(email: "email-invalido");

        Assert.Throws<InvalidEmailException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidPasswordException_WhenPasswordHashIsEmpty()
    {
        var act = () => AuthTestData.CreateUser(passwordHash: "");

        Assert.Throws<InvalidPasswordException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidPasswordException_WhenPasswordHashIsWhitespace()
    {
        var act = () => AuthTestData.CreateUser(passwordHash: "   ");

        Assert.Throws<InvalidPasswordException>(act);
    }
}
