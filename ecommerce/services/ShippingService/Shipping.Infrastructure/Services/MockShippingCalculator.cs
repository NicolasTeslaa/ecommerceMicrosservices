using Shipping.Application.Interfaces;
using Shipping.Domain.Models;
using System.Diagnostics;

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
        {
            LogSoftFailure($"MockShippingCalculator received unsupported provider '{provider}'. Falling back to mock.");
            provider = "mock";
        }

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
            LogSoftFailure("MockShippingCalculator received a non-positive height.");
        if (widthCm <= 0)
            LogSoftFailure("MockShippingCalculator received a non-positive width.");
        if (cubageM3 <= 0)
            LogSoftFailure("MockShippingCalculator received a non-positive cubage.");
        if (weightKg <= 0)
            LogSoftFailure("MockShippingCalculator received a non-positive weight.");
        if (string.IsNullOrWhiteSpace(originZipCode))
            LogSoftFailure("MockShippingCalculator received an empty origin zip code.");
        if (string.IsNullOrWhiteSpace(destinationZipCode))
            LogSoftFailure("MockShippingCalculator received an empty destination zip code.");
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
        var normalized = new string((zipCode ?? string.Empty).Where(char.IsDigit).ToArray());

        if (normalized.Length < 8)
            normalized = normalized.PadLeft(8, '0');

        return normalized;
    }

    private static void LogSoftFailure(string message) => Trace.TraceError(message);
}
