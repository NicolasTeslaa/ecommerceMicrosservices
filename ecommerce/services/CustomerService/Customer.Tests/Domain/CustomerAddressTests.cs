using Customer.Domain.Entities;
using Customer.Domain.Exceptions;

namespace Customer.Tests.Domain;

public class CustomerAddressTests
{
    [Fact]
    public void Constructor_ShouldCreateAddress_WhenDataIsValid()
    {
        var address = CreateAddress();

        Assert.NotEqual(Guid.Empty, address.Id);
        Assert.Equal("Casa", address.Label);
        Assert.Equal("Jane Doe", address.RecipientName);
        Assert.True(address.IsDefault);
    }

    [Fact]
    public void Constructor_ShouldTrimFields_WhenValuesContainWhitespace()
    {
        var address = new CustomerAddress(
            Guid.NewGuid(),
            " Casa ",
            " Jane Doe ",
            " Rua A ",
            " 123 ",
            " Apto 1 ",
            " Centro ",
            " Sao Paulo ",
            " SP ",
            " 01000-000 ",
            " Brasil ",
            " Portao azul ",
            false);

        Assert.Equal("Casa", address.Label);
        Assert.Equal("Jane Doe", address.RecipientName);
        Assert.Equal("Rua A", address.Street);
        Assert.Equal("123", address.Number);
        Assert.Equal("Apto 1", address.Complement);
        Assert.Equal("Centro", address.Neighborhood);
        Assert.Equal("Sao Paulo", address.City);
        Assert.Equal("SP", address.State);
        Assert.Equal("01000-000", address.ZipCode);
        Assert.Equal("Brasil", address.Country);
        Assert.Equal("Portao azul", address.Reference);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidAddressLabelException_WhenLabelIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "", "Jane", "Rua", "1", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);

        Assert.Throws<InvalidAddressLabelException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidRecipientNameException_WhenRecipientNameIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "Casa", "", "Rua", "1", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);

        Assert.Throws<InvalidRecipientNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidStreetException_WhenStreetIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "Casa", "Jane", "", "1", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);

        Assert.Throws<InvalidStreetException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidNumberException_WhenNumberIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "Casa", "Jane", "Rua", "", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);

        Assert.Throws<InvalidNumberException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidNeighborhoodException_WhenNeighborhoodIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "Casa", "Jane", "Rua", "1", "", "", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);

        Assert.Throws<InvalidNeighborhoodException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCityException_WhenCityIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "Casa", "Jane", "Rua", "1", "", "Centro", "", "SP", "01000-000", "Brasil", "", false);

        Assert.Throws<InvalidCityException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidStateException_WhenStateIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "Casa", "Jane", "Rua", "1", "", "Centro", "Sao Paulo", "", "01000-000", "Brasil", "", false);

        Assert.Throws<InvalidStateException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidZipCodeException_WhenZipCodeIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "Casa", "Jane", "Rua", "1", "", "Centro", "Sao Paulo", "SP", "", "Brasil", "", false);

        Assert.Throws<InvalidZipCodeException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCountryException_WhenCountryIsEmpty()
    {
        var act = () => new CustomerAddress(Guid.NewGuid(), "Casa", "Jane", "Rua", "1", "", "Centro", "Sao Paulo", "SP", "01000-000", "", "", false);

        Assert.Throws<InvalidCountryException>(act);
    }

    [Fact]
    public void Update_ShouldRefreshFields_WhenDataIsValid()
    {
        var address = CreateAddress();

        address.Update("Trabalho", "Jane Silva", "Rua B", "45", "Sala 2", "Bairro", "Campinas", "SP", "13000-000", "Brasil", "Casa cinza", false);

        Assert.Equal("Trabalho", address.Label);
        Assert.Equal("Jane Silva", address.RecipientName);
        Assert.Equal("Rua B", address.Street);
        Assert.Equal("45", address.Number);
        Assert.False(address.IsDefault);
    }

    [Fact]
    public void Update_ShouldChangeUpdatedAtUtc_WhenAddressIsUpdated()
    {
        var address = CreateAddress();
        var originalUpdatedAt = address.UpdatedAtUtc;

        Thread.Sleep(5);
        address.Update("Trabalho", "Jane Silva", "Rua B", "45", "Sala 2", "Bairro", "Campinas", "SP", "13000-000", "Brasil", "Casa cinza", false);

        Assert.True(address.UpdatedAtUtc >= originalUpdatedAt);
    }

    [Fact]
    public void SetDefault_ShouldMarkAddressAsDefault()
    {
        var address = CreateAddress(false);

        address.SetDefault(true);

        Assert.True(address.IsDefault);
    }

    [Fact]
    public void SetDefault_ShouldUpdateUpdatedAtUtc()
    {
        var address = CreateAddress(false);
        var originalUpdatedAt = address.UpdatedAtUtc;

        Thread.Sleep(5);
        address.SetDefault(true);

        Assert.True(address.UpdatedAtUtc >= originalUpdatedAt);
    }

    [Fact]
    public void ToSingleLine_ShouldIncludeComplementAndReference_WhenTheyExist()
    {
        var address = CreateAddress();

        var result = address.ToSingleLine();

        Assert.Contains("Apto 1", result);
        Assert.Contains("(Portao azul)", result);
    }

    [Fact]
    public void ToSingleLine_ShouldOmitComplementAndReference_WhenTheyAreEmpty()
    {
        var address = new CustomerAddress(
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

        var result = address.ToSingleLine();

        Assert.DoesNotContain("()", result);
        Assert.DoesNotContain(", ,", result);
    }

    private static CustomerAddress CreateAddress(bool isDefault = true)
    {
        return new CustomerAddress(
            Guid.NewGuid(),
            "Casa",
            "Jane Doe",
            "Rua A",
            "123",
            "Apto 1",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            "Brasil",
            "Portao azul",
            isDefault);
    }
}
