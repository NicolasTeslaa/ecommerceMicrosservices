using Moq;
using Shipping.Application.Commands;
using Shipping.Application.Handlers;
using Shipping.Application.Interfaces;
using Shipping.Domain.Models;

namespace Shipping.Tests.Handlers;

public class CalculateShippingHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMapQuoteReturnedByCalculator()
    {
        var calculatorMock = new Mock<IShippingCalculator>();
        var command = new CalculateShippingCommand
        {
            HeightCm = 12m,
            WidthCm = 18m,
            CubageM3 = 0.45m,
            WeightKg = 2.5m,
            OriginZipCode = "01001-000",
            DestinationZipCode = "01021-000",
            Provider = "mock"
        };
        var quote = new ShippingQuote("MockShippingCalculator", 42.75m, 3);
        var handler = new CalculateShippingHandler(calculatorMock.Object);

        calculatorMock
            .Setup(calculator => calculator.CalculateAsync(
                command.HeightCm,
                command.WidthCm,
                command.CubageM3,
                command.WeightKg,
                command.OriginZipCode,
                command.DestinationZipCode,
                command.Provider,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(quote.Provider, result.Provider);
        Assert.Equal(quote.Amount, result.Amount);
        Assert.Equal(quote.EstimatedDays, result.EstimatedDays);
        Assert.Equal(quote.EstimatedDeliveryDescription, result.EstimatedDeliveryDescription);
    }

    [Fact]
    public async Task Handle_ShouldForwardAllParametersToCalculator()
    {
        var calculatorMock = new Mock<IShippingCalculator>();
        var command = new CalculateShippingCommand
        {
            HeightCm = 8m,
            WidthCm = 14m,
            CubageM3 = 0.22m,
            WeightKg = 1.8m,
            OriginZipCode = "01153-000",
            DestinationZipCode = "01311-000",
            Provider = "mock"
        };
        var handler = new CalculateShippingHandler(calculatorMock.Object);

        calculatorMock
            .Setup(calculator => calculator.CalculateAsync(
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingQuote("MockShippingCalculator", 20m, 2));

        await handler.Handle(command, CancellationToken.None);

        calculatorMock.Verify(calculator => calculator.CalculateAsync(
            command.HeightCm,
            command.WidthCm,
            command.CubageM3,
            command.WeightKg,
            command.OriginZipCode,
            command.DestinationZipCode,
            command.Provider,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
