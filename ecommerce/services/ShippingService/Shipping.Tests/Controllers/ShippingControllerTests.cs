using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shipping.API.Controllers;
using Shipping.Application.Commands;
using Shipping.Application.DTOs;

namespace Shipping.Tests.Controllers;

public class ShippingControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly ShippingController _controller;

    public ShippingControllerTests()
    {
        _controller = new ShippingController(_mediatorMock.Object);
    }

    [Fact]
    public async Task CalculateQuote_ShouldReturnOkResponse_WhenMediatorReturnsQuote()
    {
        var command = new CalculateShippingCommand
        {
            HeightCm = 15m,
            WidthCm = 10m,
            CubageM3 = 0.4m,
            WeightKg = 2m,
            OriginZipCode = "01001-000",
            DestinationZipCode = "01311-000",
            Provider = "mock"
        };
        var quote = new ShippingQuoteDto
        {
            Provider = "MockShippingCalculator",
            Amount = 55.40m,
            EstimatedDays = 2,
            EstimatedDeliveryDescription = "2 dias"
        };

        _mediatorMock
            .Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        var result = await _controller.CalculateQuote(command);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<ApiResponse<ShippingQuoteDto>>(response.Value);
        Assert.True(payload.Success);
        Assert.Equal(quote.Provider, payload.Data!.Provider);
        Assert.Equal("Shipping quote calculated successfully.", payload.Message);
    }

    [Fact]
    public async Task CalculateQuote_ShouldForwardCommandToMediator()
    {
        var command = new CalculateShippingCommand
        {
            HeightCm = 10m,
            WidthCm = 10m,
            CubageM3 = 0.2m,
            WeightKg = 1m,
            OriginZipCode = "01001-000",
            DestinationZipCode = "01311-000",
            Provider = "mock"
        };

        _mediatorMock
            .Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingQuoteDto
            {
                Provider = "MockShippingCalculator",
                Amount = 30m,
                EstimatedDays = 2,
                EstimatedDeliveryDescription = "2 dias"
            });

        await _controller.CalculateQuote(command);

        _mediatorMock.Verify(mediator => mediator.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
