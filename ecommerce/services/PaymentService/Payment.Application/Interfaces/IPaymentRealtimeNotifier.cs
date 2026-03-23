namespace Payment.Application.Interfaces;

public interface IPaymentRealtimeNotifier
{
    Task NotifyUpdatedAsync(Guid orderId, CancellationToken cancellationToken = default);
}
