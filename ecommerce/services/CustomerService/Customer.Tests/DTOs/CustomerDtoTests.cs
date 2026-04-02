using Customer.Application.DTOs;
using Customer.Tests.Support;

namespace Customer.Tests.DTOs;

public class CustomerDtoTests
{
    [Fact]
    public void MapFromEntity_ShouldMapCustomerFields()
    {
        var customer = CustomerTestData.CreateCustomer();

        var dto = CustomerDto.MapFromEntity(customer);

        Assert.Equal(customer.Id, dto.Id);
        Assert.Equal(customer.AuthUserId, dto.AuthUserId);
        Assert.Equal(customer.FullName, dto.FullName);
        Assert.Equal(customer.Email, dto.Email);
        Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);
    }

    [Fact]
    public void MapFromEntity_ShouldMapAddresses()
    {
        var customer = CustomerTestData.CreateCustomer();
        CustomerTestData.AddAddress(customer, true);
        CustomerTestData.AddAddress(customer, false);

        var dto = CustomerDto.MapFromEntity(customer);

        Assert.Equal(2, dto.Addresses.Count);
    }

    [Fact]
    public void MapFromEntity_ShouldReturnEmptyAddresses_WhenCustomerHasNoAddresses()
    {
        var customer = CustomerTestData.CreateCustomer();

        var dto = CustomerDto.MapFromEntity(customer);

        Assert.Empty(dto.Addresses);
    }
}
