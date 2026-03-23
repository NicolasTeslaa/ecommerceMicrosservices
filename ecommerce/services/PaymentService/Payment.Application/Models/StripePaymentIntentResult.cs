namespace Payment.Application.Models;

public class StripePaymentIntentResult
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethodId { get; set; }
}
