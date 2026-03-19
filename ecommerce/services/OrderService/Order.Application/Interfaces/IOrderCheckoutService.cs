using Order.Application.Commands;
using Order.Application.DTOs;

namespace Order.Application.Interfaces;

public interface IOrderCheckoutService
{
    Task<OrderProcessingAcceptedDto> QueueOrderAsync(CreateOrderCommand request, CancellationToken cancellationToken = default);
}
