using Shipping.Application.Interfaces;
using Shipping.Domain.Exceptions;
using Shipping.Domain.Models;

namespace Shipping.Infrastructure.Services;

public class MockShippingCalculator : IShippingCalculator
{
    public Task<ShippingQuote> CalculateAsync(
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode,
        string destinationZipCode,
        string provider,
        CancellationToken cancellationToken = default)
    {
        Validate(heightCm, widthCm, cubageM3, weightKg, originZipCode, destinationZipCode);

        if (!string.Equals(provider, "mock", StringComparison.OrdinalIgnoreCase))
            throw new ProviderNotSupportedException(provider);

        var distanceFactor = CalculateDistanceFactor(originZipCode, destinationZipCode);
        var amount = 12m + (distanceFactor * 3.75m) + (weightKg * 2.40m) + (cubageM3 * 420m) + ((heightCm + widthCm) * 0.08m);
        var estimatedDays = distanceFactor switch
        {
            <= 3 => 2,
            <= 8 => 4,
            <= 15 => 7,
            _ => 10
        };

        return Task.FromResult(new ShippingQuote("MockShippingCalculator", amount, estimatedDays));
    }

    private static void Validate(
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode,
        string destinationZipCode)
    {
        if (heightCm <= 0)
            throw new InvalidHeightException();
        if (widthCm <= 0)
            throw new InvalidWidthException();
        if (cubageM3 <= 0)
            throw new InvalidCubageException();
        if (weightKg <= 0)
            throw new InvalidWeightException();
        if (string.IsNullOrWhiteSpace(originZipCode))
            throw new InvalidOriginZipCodeException();
        if (string.IsNullOrWhiteSpace(destinationZipCode))
            throw new InvalidDestinationZipCodeException();
    }

    private static int CalculateDistanceFactor(string originZipCode, string destinationZipCode)
    {
        var origin = NormalizeZipCode(originZipCode);
        var destination = NormalizeZipCode(destinationZipCode);

        var originPrefix = int.Parse(origin[..3]);
        var destinationPrefix = int.Parse(destination[..3]);
        var originSuffix = int.Parse(origin[3..5]);
        var destinationSuffix = int.Parse(destination[3..5]);

        var prefixDistance = Math.Abs(originPrefix - destinationPrefix);
        var suffixDistance = Math.Abs(originSuffix - destinationSuffix);

        return prefixDistance + (suffixDistance / 10);
    }

    private static string NormalizeZipCode(string zipCode)
    {
        var normalized = new string(zipCode.Where(char.IsDigit).ToArray());

        if (normalized.Length < 8)
            normalized = normalized.PadLeft(8, '0');

        return normalized;
    }
}
