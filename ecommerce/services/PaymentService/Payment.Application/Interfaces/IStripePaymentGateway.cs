using Payment.Application.Models;

namespace Payment.Application.Interfaces;

public interface IStripePaymentGateway
{
    Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
}
