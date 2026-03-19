using MediatR;
using Shipping.Application.DTOs;

namespace Shipping.Application.Commands;

public class CalculateShippingCommand : IRequest<ShippingQuoteDto>
{
    public decimal HeightCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal CubageM3 { get; set; }
    public decimal WeightKg { get; set; }
    public string OriginZipCode { get; set; } = string.Empty;
    public string DestinationZipCode { get; set; } = string.Empty;
    public string Provider { get; set; } = "mock";
}
