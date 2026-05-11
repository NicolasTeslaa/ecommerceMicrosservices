using System.Diagnostics;

namespace Shipping.Domain.Models;

public class ShippingQuote
{
    public string Provider { get; }
    public decimal Amount { get; }
    public int EstimatedDays { get; }
    public string EstimatedDeliveryDescription => $"{EstimatedDays} dias";

    public ShippingQuote(string provider, decimal amount, int estimatedDays)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            Trace.TraceError("ShippingQuote received an empty provider. Falling back to 'unknown'.");
            provider = "unknown";
        }

        Provider = provider.Trim();
        Amount = decimal.Round(Math.Max(amount, 0.01m), 2, MidpointRounding.AwayFromZero);
        EstimatedDays = Math.Max(estimatedDays, 1);
    }
}
