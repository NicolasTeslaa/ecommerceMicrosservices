using Payment.Application.Interfaces;
using Payment.Application.Models;
using Payment.Infrastructure.Configuration;
using Stripe;

namespace Payment.Infrastructure.Clients;

public class StripePaymentGateway : IStripePaymentGateway
{
    private readonly StripeOptions _options;

    public StripePaymentGateway(Microsoft.Extensions.Options.IOptions<StripeOptions> options)
    {
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.SecretKey))
            StripeConfiguration.ApiKey = _options.SecretKey;
    }

    public async Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        var amountInCents = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

        var paymentIntent = await service.CreateAsync(
            new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = currency.ToLowerInvariant(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = orderId.ToString(),
                    ["customerId"] = customerId.ToString()
                }
            },
            cancellationToken: cancellationToken);

        return new StripePaymentIntentResult
        {
            PaymentIntentId = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret,
            Status = paymentIntent.Status,
            PaymentMethodId = paymentIntent.PaymentMethodId
        };
    }
}
