using System.Net;
using IntegrationTests.Contracts;
using IntegrationTests.Infrastructure;

namespace IntegrationTests;

[Collection(MicroservicesCollection.Name)]
public sealed class AuthIntegrationTests
{
    private readonly MicroservicesTestEnvironment _environment;

    public AuthIntegrationTests(MicroservicesTestEnvironment environment)
    {
        _environment = environment;
    }

    [Fact]
    public async Task RegisteredUser_ShouldBeAbleToLogin()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var registerRequest = IntegrationTestData.CreateRegisterUserRequest(suffix);

        var (registerStatusCode, registerBody) = await _environment.GatewayApi.RegisterUserAsync(registerRequest);

        Assert.Equal(HttpStatusCode.Created, registerStatusCode);
        Assert.NotNull(registerBody?.Data);

        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var (loginStatusCode, loginBody) = await _environment.GatewayApi.LoginAsync(loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginStatusCode);
        Assert.NotNull(loginBody);
        Assert.True(loginBody!.Success, loginBody.Message);
        Assert.NotNull(loginBody.Data);

        var loginData = loginBody.Data!;
        var registerData = registerBody!.Data!;

        Assert.Equal(registerData.UserId, loginData.UserId);
        Assert.Equal(registerData.CustomerId, loginData.CustomerId);
        Assert.Equal(registerRequest.FullName, loginData.FullName);
        Assert.Equal(registerRequest.Email.ToLowerInvariant(), loginData.Email);
        Assert.False(string.IsNullOrWhiteSpace(loginData.AccessToken));
        Assert.True(loginData.ExpiresAtUtc > DateTime.UtcNow);
    }
}
