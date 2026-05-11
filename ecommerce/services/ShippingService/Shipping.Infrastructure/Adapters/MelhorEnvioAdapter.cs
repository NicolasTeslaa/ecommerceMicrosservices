using System.Diagnostics;
using Shipping.Application.Interfaces;
using Shipping.Domain.Models;

namespace Shipping.Infrastructure.Adapters;

public class MelhorEnvioAdapter : IShippingProviderAdapter
{
    public string ProviderName => "melhorenvio";

    public Task<ShippingQuote> CalculateAsync(decimal heightCm, decimal widthCm, decimal cubageM3, decimal weightKg, string originZipCode, string destinationZipCode, CancellationToken cancellationToken = default)
    {
        Trace.TraceError("Provider {0} is not supported. Returning fallback shipping quote.", ProviderName);
        return Task.FromResult(new ShippingQuote(ProviderName, 0m, 0, "Provider not supported."));
    }
}
