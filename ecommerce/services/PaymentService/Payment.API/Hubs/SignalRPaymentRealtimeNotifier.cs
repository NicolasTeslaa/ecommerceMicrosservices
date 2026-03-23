using Microsoft.AspNetCore.SignalR;
using Payment.Application.Interfaces;

namespace Payment.API.Hubs;

public class SignalRPaymentRealtimeNotifier : IPaymentRealtimeNotifier
{
    private readonly IHubContext<PaymentStatusHub> _hubContext;

    public SignalRPaymentRealtimeNotifier(IHubContext<PaymentStatusHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyUpdatedAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(PaymentStatusHub.BuildOrderGroup(orderId))
            .SendAsync("payment-updated", orderId, cancellationToken);
    }
}
