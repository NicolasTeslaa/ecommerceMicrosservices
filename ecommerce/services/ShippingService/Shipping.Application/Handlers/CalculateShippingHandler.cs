using MediatR;
using Shipping.Application.Commands;
using Shipping.Application.DTOs;
using Shipping.Application.Interfaces;

namespace Shipping.Application.Handlers;

public class CalculateShippingHandler : IRequestHandler<CalculateShippingCommand, ShippingQuoteDto>
{
    private readonly IShippingCalculator _calculator;

    public CalculateShippingHandler(IShippingCalculator calculator)
    {
        _calculator = calculator;
    }

    public async Task<ShippingQuoteDto> Handle(CalculateShippingCommand request, CancellationToken cancellationToken)
    {
        var quote = await _calculator.CalculateAsync(
            request.HeightCm,
            request.WidthCm,
            request.CubageM3,
            request.WeightKg,
            request.OriginZipCode,
            request.DestinationZipCode,
            request.Provider,
            cancellationToken);

        return ShippingQuoteDto.MapFromDomain(quote);
    }
}
