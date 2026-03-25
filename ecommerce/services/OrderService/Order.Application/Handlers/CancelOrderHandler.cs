using MediatR;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Exceptions;

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
            throw new InvalidOrderIdException();

        if (request.CustomerId == Guid.Empty)
            throw new InvalidCustomerIdException();

        return await _cancellationService.CancelAsync(request.OrderId, request.CustomerId, cancellationToken);
    }
}
