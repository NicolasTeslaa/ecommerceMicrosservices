namespace Payment.Application.Interfaces;

public interface IPaymentEventPublisher
{
    Task PublishApprovedAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default);
    Task PublishFailedAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default);
}
