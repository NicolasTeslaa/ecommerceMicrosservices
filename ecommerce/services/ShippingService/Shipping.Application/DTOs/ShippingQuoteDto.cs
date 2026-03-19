using Shipping.Domain.Models;

namespace Shipping.Application.DTOs;

public class ShippingQuoteDto
{
    public string Provider { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int EstimatedDays { get; set; }
    public string EstimatedDeliveryDescription { get; set; } = string.Empty;

    public static ShippingQuoteDto MapFromDomain(ShippingQuote quote)
    {
        return new ShippingQuoteDto
        {
            Provider = quote.Provider,
            Amount = quote.Amount,
            EstimatedDays = quote.EstimatedDays,
            EstimatedDeliveryDescription = quote.EstimatedDeliveryDescription
        };
    }
}
