using Shipping.Domain.Models;

namespace Shipping.Application.Interfaces;

public interface IShippingCalculator
{
    Task<ShippingQuote> CalculateAsync(
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode,
        string destinationZipCode,
        string provider,
        CancellationToken cancellationToken = default);
}
