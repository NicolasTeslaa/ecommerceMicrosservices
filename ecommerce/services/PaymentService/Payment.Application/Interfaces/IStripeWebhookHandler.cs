namespace Payment.Application.Interfaces;

public interface IStripeWebhookHandler
{
    Task HandleAsync(string jsonPayload, string? signatureHeader, CancellationToken cancellationToken = default);
}
