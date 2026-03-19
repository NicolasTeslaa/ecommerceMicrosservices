using Shipping.Application.Interfaces;
using Shipping.Domain.Exceptions;
using Shipping.Domain.Models;

namespace Shipping.Infrastructure.Adapters;

public class PSFreteAdapter : IShippingProviderAdapter
{
    public string ProviderName => "psfrete";

    public Task<ShippingQuote> CalculateAsync(decimal heightCm, decimal widthCm, decimal cubageM3, decimal weightKg, string originZipCode, string destinationZipCode, CancellationToken cancellationToken = default)
        => throw new ProviderNotSupportedException(ProviderName);
}
