namespace Payment.Application.Interfaces;

public interface IOrderPaymentAccessClient
{
    Task<(bool OrderExists, bool CustomerMatches)> ValidateAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default);
}
