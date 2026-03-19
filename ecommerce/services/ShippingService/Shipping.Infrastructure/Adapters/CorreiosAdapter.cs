using Shipping.Application.Interfaces;
using Shipping.Domain.Exceptions;
using Shipping.Domain.Models;

namespace Shipping.Infrastructure.Adapters;

public class CorreiosAdapter : IShippingProviderAdapter
{
    public string ProviderName => "correios";

    public Task<ShippingQuote> CalculateAsync(decimal heightCm, decimal widthCm, decimal cubageM3, decimal weightKg, string originZipCode, string destinationZipCode, CancellationToken cancellationToken = default)
        => throw new ProviderNotSupportedException(ProviderName);
}
