namespace Payment.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment.Domain.Entities.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Payment.Domain.Entities.Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default);
}
