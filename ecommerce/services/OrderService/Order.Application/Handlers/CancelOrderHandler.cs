using MediatR;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Application.Handlers;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, OrderActionResultDto>
{
    private readonly IOrderCancellationService _cancellationService;

    public CancelOrderHandler(IOrderCancellationService cancellationService)
    {
        _cancellationService = cancellationService;
    }

    public async Task<OrderActionResultDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            return new OrderActionResultDto { OrderId = Guid.Empty, Message = "Invalid order id.", Status = string.Empty };

        if (request.CustomerId == Guid.Empty)
            return new OrderActionResultDto { OrderId = request.OrderId, Message = "Invalid customer id.", Status = string.Empty };

        return await _cancellationService.CancelAsync(request.OrderId, request.CustomerId, cancellationToken);
    }
}
