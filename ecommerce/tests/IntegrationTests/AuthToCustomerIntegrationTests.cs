using System.Net;
using IntegrationTests.Contracts;
using IntegrationTests.Infrastructure;

namespace IntegrationTests;

[Collection(MicroservicesCollection.Name)]
public sealed class AuthToCustomerIntegrationTests
{
    private readonly MicroservicesTestEnvironment _environment;

    public AuthToCustomerIntegrationTests(MicroservicesTestEnvironment environment)
    {
        _environment = environment;
    }

    [Fact]
    public async Task RegisteringUser_ShouldEventuallyCreateCustomerInCustomerService()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var request = IntegrationTestData.CreateRegisterUserRequest(suffix);

        var (registerStatusCode, registerBody) = await _environment.GatewayApi.RegisterUserAsync(request);

        Assert.Equal(HttpStatusCode.Created, registerStatusCode);
        Assert.NotNull(registerBody);
        Assert.True(registerBody!.Success, registerBody.Message);
        Assert.NotNull(registerBody.Data);

        var authData = registerBody.Data!;
        Assert.NotEqual(Guid.Empty, authData.CustomerId);
        Assert.Equal(request.Email.ToLowerInvariant(), authData.Email);

        await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.GetCustomerAsync(authData.CustomerId, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK && result.Body?.Success == true);

        var (customerStatusCode, customerBody) = await _environment.GatewayApi.GetCustomerAsync(authData.CustomerId);

        Assert.Equal(HttpStatusCode.OK, customerStatusCode);
        Assert.NotNull(customerBody);
        Assert.True(customerBody!.Success, customerBody.Message);
        Assert.NotNull(customerBody.Data);

        var customer = customerBody.Data!;
        Assert.Equal(authData.CustomerId, customer.Id);
        Assert.Equal(authData.UserId, customer.AuthUserId);
        Assert.Equal(request.FullName, customer.FullName);
        Assert.Equal(request.Email.ToLowerInvariant(), customer.Email);
        Assert.Equal(IntegrationTestData.NormalizePhoneNumber(request.PhoneNumber), customer.PhoneNumber);
    }
}
