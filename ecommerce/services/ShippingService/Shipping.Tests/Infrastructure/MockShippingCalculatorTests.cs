using Shipping.Domain.Exceptions;
using Shipping.Infrastructure.Services;

namespace Shipping.Tests.Infrastructure;

public class MockShippingCalculatorTests
{
    [Fact]
    public async Task CalculateAsync_ShouldReturnDeterministicQuote_WhenRequestIsValid()
    {
        var calculator = new MockShippingCalculator();

        var quote = await calculator.CalculateAsync(
            10m,
            20m,
            0.5m,
            2m,
            "01001-000",
            "01021-000",
            "mock",
            CancellationToken.None);

        Assert.Equal("MockShippingCalculator", quote.Provider);
        Assert.Equal(236.70m, quote.Amount);
        Assert.Equal(2, quote.EstimatedDays);
        Assert.Equal("2 dias", quote.EstimatedDeliveryDescription);
    }

    [Fact]
    public async Task CalculateAsync_ShouldThrowProviderNotSupportedException_WhenProviderIsUnsupported()
    {
        var calculator = new MockShippingCalculator();

        var act = () => calculator.CalculateAsync(
            10m,
            20m,
            0.5m,
            2m,
            "01001-000",
            "01021-000",
            "correios",
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ProviderNotSupportedException>(act);
        Assert.Equal("Shipping provider 'correios' is not supported.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CalculateAsync_ShouldThrowInvalidHeightException_WhenHeightIsNotPositive(decimal height)
    {
        var calculator = new MockShippingCalculator();

        var act = () => calculator.CalculateAsync(
            height,
            20m,
            0.5m,
            2m,
            "01001-000",
            "01021-000",
            "mock",
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidHeightException>(act);
    }
}
