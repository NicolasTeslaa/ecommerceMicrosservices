using System.Net;
using IntegrationTests.Contracts;
using IntegrationTests.Infrastructure;

namespace IntegrationTests;

[Collection(MicroservicesCollection.Name)]
public sealed class CustomerAddressIntegrationTests
{
    private readonly MicroservicesTestEnvironment _environment;

    public CustomerAddressIntegrationTests(MicroservicesTestEnvironment environment)
    {
        _environment = environment;
    }

    [Fact]
    public async Task RegisteredCustomer_ShouldBeAbleToCreateAndListAddresses()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var registerRequest = IntegrationTestData.CreateRegisterUserRequest(suffix);

        var (registerStatusCode, registerBody) = await _environment.GatewayApi.RegisterUserAsync(registerRequest);

        Assert.Equal(HttpStatusCode.Created, registerStatusCode);
        Assert.NotNull(registerBody?.Data);

        var customerId = registerBody!.Data!.CustomerId;

        await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.GetCustomerAsync(customerId, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK && result.Body?.Success == true);

        var addressRequest = IntegrationTestData.CreateAddressRequest();
        var (createStatusCode, createBody) = await _environment.GatewayApi.AddCustomerAddressAsync(customerId, addressRequest);

        Assert.Equal(HttpStatusCode.OK, createStatusCode);
        Assert.NotNull(createBody?.Data);
        Assert.True(createBody!.Success, createBody.Message);

        var createdAddress = createBody.Data!;
        Assert.Equal(customerId, createdAddress.CustomerId);
        Assert.Equal(addressRequest.Label, createdAddress.Label);
        Assert.True(createdAddress.IsDefault);

        var (listStatusCode, listBody) = await _environment.GatewayApi.GetCustomerAddressesAsync(customerId);

        Assert.Equal(HttpStatusCode.OK, listStatusCode);
        Assert.NotNull(listBody?.Data);
        Assert.True(listBody!.Success, listBody.Message);

        var addresses = listBody.Data!;
        var storedAddress = Assert.Single(addresses, address => address.Id == createdAddress.Id);
        Assert.Equal(addressRequest.RecipientName, storedAddress.RecipientName);
        Assert.Equal(addressRequest.ZipCode, storedAddress.ZipCode);
        Assert.Equal(addressRequest.Country, storedAddress.Country);
        Assert.True(storedAddress.IsDefault);
    }

    [Fact]
    public async Task Customer_ShouldBeAbleToUpdateSetDefaultAndRemoveAddresses()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var registerRequest = IntegrationTestData.CreateRegisterUserRequest(suffix);

        var (registerStatusCode, registerBody) = await _environment.GatewayApi.RegisterUserAsync(registerRequest);

        Assert.Equal(HttpStatusCode.Created, registerStatusCode);
        Assert.NotNull(registerBody?.Data);

        var customerId = registerBody!.Data!.CustomerId;

        await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.GetCustomerAsync(customerId, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK && result.Body?.Success == true);

        var primaryAddressRequest = IntegrationTestData.CreateAddressRequest();
        var secondaryAddressRequest = IntegrationTestData.CreateAddressRequest(isDefault: false);
        secondaryAddressRequest.Label = "Trabalho";
        secondaryAddressRequest.Number = "500";
        secondaryAddressRequest.Reference = "Recepcao";

        var primaryCreateResult = await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.AddCustomerAddressAsync(customerId, primaryAddressRequest, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK && result.Body?.Success == true && result.Body.Data is not null,
            describe: result => $"StatusCode={result.StatusCode}, Success={result.Body?.Success}, Message={result.Body?.Message}, Error={result.Body?.Error?.Code}");

        var secondaryCreateResult = await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.AddCustomerAddressAsync(customerId, secondaryAddressRequest, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK && result.Body?.Success == true && result.Body.Data is not null,
            describe: result => $"StatusCode={result.StatusCode}, Success={result.Body?.Success}, Message={result.Body?.Message}, Error={result.Body?.Error?.Code}");

        var primaryAddress = primaryCreateResult.Body!.Data!;
        var secondaryAddress = secondaryCreateResult.Body!.Data!;

        var updateRequest = IntegrationTestData.CreateAddressRequest(isDefault: false);
        updateRequest.Label = "Escritorio";
        updateRequest.Street = "Avenida Atualizada";
        updateRequest.Number = "999";

        var (updateStatusCode, updateBody) = await _environment.GatewayApi.UpdateCustomerAddressAsync(
            customerId,
            secondaryAddress.Id,
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateStatusCode);
        Assert.NotNull(updateBody?.Data);
        Assert.Equal(updateRequest.Label, updateBody!.Data!.Label);
        Assert.Equal(updateRequest.Street, updateBody.Data.Street);
        Assert.False(updateBody.Data.IsDefault);

        var (setDefaultStatusCode, setDefaultBody) = await _environment.GatewayApi.SetDefaultCustomerAddressAsync(customerId, secondaryAddress.Id);

        Assert.Equal(HttpStatusCode.OK, setDefaultStatusCode);
        Assert.NotNull(setDefaultBody?.Data);
        Assert.True(setDefaultBody!.Data!.IsDefault);
        Assert.Equal(secondaryAddress.Id, setDefaultBody.Data.Id);

        var (removeStatusCode, removeBody) = await _environment.GatewayApi.RemoveCustomerAddressAsync(customerId, primaryAddress.Id);

        Assert.Equal(HttpStatusCode.OK, removeStatusCode);
        Assert.NotNull(removeBody);
        Assert.True(removeBody!.Success, removeBody.Message);

        var (listStatusCode, listBody) = await _environment.GatewayApi.GetCustomerAddressesAsync(customerId);

        Assert.Equal(HttpStatusCode.OK, listStatusCode);
        Assert.NotNull(listBody?.Data);

        var remainingAddress = Assert.Single(listBody!.Data!);
        Assert.Equal(secondaryAddress.Id, remainingAddress.Id);
        Assert.Equal(updateRequest.Label, remainingAddress.Label);
        Assert.True(remainingAddress.IsDefault);
    }
}
