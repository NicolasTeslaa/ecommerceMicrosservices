using Order.Application.DTOs;

namespace Order.Application.Interfaces;

public interface IOrderCancellationService
{
    Task<OrderActionResultDto> CancelAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken = default);
}
