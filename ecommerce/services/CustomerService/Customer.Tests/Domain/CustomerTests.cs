using Customer.Domain.Exceptions;
using Customer.Tests.Support;

namespace Customer.Tests.Domain;

public class CustomerTests
{
    [Fact]
    public void Constructor_ShouldCreateCustomer_WhenDataIsValid()
    {
        var customer = CustomerTestData.CreateCustomer();

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Jane Doe", customer.FullName);
        Assert.Equal("jane@example.com", customer.Email);
    }

    [Fact]
    public void Constructor_ShouldTrimFullName_AndNormalizeEmail()
    {
        var customer = CustomerTestData.CreateCustomer(fullName: " Jane Doe ", email: " Jane@Example.Com ");

        Assert.Equal("Jane Doe", customer.FullName);
        Assert.Equal("jane@example.com", customer.Email);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCustomerNameException_WhenNameIsEmpty()
    {
        var act = () => CustomerTestData.CreateCustomer(fullName: "");

        Assert.Throws<InvalidCustomerNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCustomerEmailException_WhenEmailIsEmpty()
    {
        var act = () => CustomerTestData.CreateCustomer(email: "");

        Assert.Throws<InvalidCustomerEmailException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCustomerEmailException_WhenEmailIsInvalid()
    {
        var act = () => CustomerTestData.CreateCustomer(email: "email-invalido");

        Assert.Throws<InvalidCustomerEmailException>(act);
    }

    [Fact]
    public void AddAddress_ShouldForceFirstAddressAsDefault()
    {
        var customer = CustomerTestData.CreateCustomer();

        var address = CustomerTestData.AddAddress(customer, isDefault: false);

        Assert.True(address.IsDefault);
        Assert.Single(customer.Addresses);
    }

    [Fact]
    public void AddAddress_ShouldClearPreviousDefault_WhenNewAddressIsDefault()
    {
        var customer = CustomerTestData.CreateCustomer();
        var first = CustomerTestData.AddAddress(customer, isDefault: true);

        var second = customer.AddAddress("Trabalho", "Jane Doe", "Rua B", "99", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", true);

        Assert.False(first.IsDefault);
        Assert.True(second.IsDefault);
        Assert.Equal(2, customer.Addresses.Count);
    }

    [Fact]
    public void UpdateAddress_ShouldUpdateExistingAddressData()
    {
        var customer = CustomerTestData.CreateCustomer();
        var address = CustomerTestData.AddAddress(customer, isDefault: true);

        var updated = customer.UpdateAddress(address.Id, "Apartamento", "Jane Silva", "Rua C", "45", "Bloco 2", "Bairro", "Campinas", "SP", "13000-000", "Brasil", "Casa cinza", true);

        Assert.Equal("Apartamento", updated.Label);
        Assert.Equal("Jane Silva", updated.RecipientName);
        Assert.Equal("Rua C", updated.Street);
        Assert.Equal("45", updated.Number);
        Assert.True(updated.IsDefault);
    }

    [Fact]
    public void UpdateAddress_ShouldThrowCustomerAddressNotFoundException_WhenAddressDoesNotExist()
    {
        var customer = CustomerTestData.CreateCustomer();

        var act = () => customer.UpdateAddress(Guid.NewGuid(), "Casa", "Jane", "Rua A", "1", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);

        Assert.Throws<CustomerAddressNotFoundException>(act);
    }

    [Fact]
    public void RemoveAddress_ShouldDeleteAddress_WhenItExists()
    {
        var customer = CustomerTestData.CreateCustomer();
        var address = CustomerTestData.AddAddress(customer, isDefault: true);

        customer.RemoveAddress(address.Id);

        Assert.Empty(customer.Addresses);
    }

    [Fact]
    public void RemoveAddress_ShouldPromoteAnotherAddress_WhenDefaultIsRemoved()
    {
        var customer = CustomerTestData.CreateCustomer();
        var first = CustomerTestData.AddAddress(customer, isDefault: true);
        var second = customer.AddAddress("Trabalho", "Jane Doe", "Rua B", "99", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);

        customer.RemoveAddress(first.Id);

        Assert.Single(customer.Addresses);
        Assert.True(second.IsDefault);
    }

    [Fact]
    public void RemoveAddress_ShouldThrowCustomerAddressNotFoundException_WhenAddressDoesNotExist()
    {
        var customer = CustomerTestData.CreateCustomer();

        var act = () => customer.RemoveAddress(Guid.NewGuid());

        Assert.Throws<CustomerAddressNotFoundException>(act);
    }

    [Fact]
    public void SetDefaultAddress_ShouldMarkRequestedAddressAsDefault()
    {
        var customer = CustomerTestData.CreateCustomer();
        var first = CustomerTestData.AddAddress(customer, isDefault: true);
        var second = customer.AddAddress("Trabalho", "Jane Doe", "Rua B", "99", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);

        var result = customer.SetDefaultAddress(second.Id);

        Assert.Same(second, result);
        Assert.True(second.IsDefault);
        Assert.False(first.IsDefault);
    }

    [Fact]
    public void SetDefaultAddress_ShouldThrowCustomerAddressNotFoundException_WhenAddressDoesNotExist()
    {
        var customer = CustomerTestData.CreateCustomer();

        var act = () => customer.SetDefaultAddress(Guid.NewGuid());

        Assert.Throws<CustomerAddressNotFoundException>(act);
    }

    [Fact]
    public void GetAddress_ShouldReturnRequestedAddress_WhenAddressExists()
    {
        var customer = CustomerTestData.CreateCustomer();
        var address = CustomerTestData.AddAddress(customer);

        var result = customer.GetAddress(address.Id);

        Assert.Same(address, result);
    }

    [Fact]
    public void GetAddress_ShouldThrowCustomerAddressNotFoundException_WhenAddressDoesNotExist()
    {
        var customer = CustomerTestData.CreateCustomer();

        var act = () => customer.GetAddress(Guid.NewGuid());

        Assert.Throws<CustomerAddressNotFoundException>(act);
    }
}
