using Shipping.Domain.Exceptions;
using Shipping.Domain.Models;

namespace Shipping.Tests.Domain;

public class ShippingQuoteTests
{
    [Fact]
    public void Constructor_ShouldTrimProviderAndNormalizeAmountAndEstimatedDays()
    {
        var quote = new ShippingQuote(" mock-provider ", 10.555m, 0);

        Assert.Equal("mock-provider", quote.Provider);
        Assert.Equal(10.56m, quote.Amount);
        Assert.Equal(1, quote.EstimatedDays);
        Assert.Equal("1 dias", quote.EstimatedDeliveryDescription);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowProviderNotSupportedException_WhenProviderIsBlank(string provider)
    {
        var act = () => new ShippingQuote(provider, 10m, 2);

        var exception = Assert.Throws<ProviderNotSupportedException>(act);
        Assert.Equal("Shipping provider 'unknown' is not supported.", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldEnforceMinimumAmount()
    {
        var quote = new ShippingQuote("mock", 0m, 3);

        Assert.Equal(0.01m, quote.Amount);
    }
}
