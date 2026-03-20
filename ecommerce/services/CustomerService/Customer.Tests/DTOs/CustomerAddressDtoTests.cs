using Customer.Application.DTOs;
using Customer.Tests.Support;

namespace Customer.Tests.DTOs;

public class CustomerAddressDtoTests
{
    [Fact]
    public void MapFromEntity_ShouldMapAddressFields()
    {
        var customer = CustomerTestData.CreateCustomer();
        var address = CustomerTestData.AddAddress(customer, true);

        var dto = CustomerAddressDto.MapFromEntity(address);

        Assert.Equal(address.Id, dto.Id);
        Assert.Equal(address.CustomerId, dto.CustomerId);
        Assert.Equal(address.Label, dto.Label);
        Assert.Equal(address.RecipientName, dto.RecipientName);
        Assert.Equal(address.Street, dto.Street);
        Assert.Equal(address.ZipCode, dto.ZipCode);
    }

    [Fact]
    public void MapFromEntity_ShouldMapDefaultFlag()
    {
        var customer = CustomerTestData.CreateCustomer();
        var address = CustomerTestData.AddAddress(customer, true);

        var dto = CustomerAddressDto.MapFromEntity(address);

        Assert.True(dto.IsDefault);
    }

    [Fact]
    public void MapFromEntity_ShouldMapOptionalFields()
    {
        var address = new Customer.Domain.Entities.CustomerAddress(
            Guid.NewGuid(),
            "Casa",
            "Jane Doe",
            "Rua A",
            "123",
            "",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            "Brasil",
            "",
            false);

        var dto = CustomerAddressDto.MapFromEntity(address);

        Assert.Equal(string.Empty, dto.Complement);
        Assert.Equal(string.Empty, dto.Reference);
    }
}
