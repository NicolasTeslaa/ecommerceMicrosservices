using MediatR;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Application.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderProcessingAcceptedDto>
{
    private readonly IOrderCheckoutService _checkoutService;

    public CreateOrderHandler(IOrderCheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    public async Task<OrderProcessingAcceptedDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.CustomerAddressId == Guid.Empty)
            throw new Order.Domain.Exceptions.InvalidCustomerAddressIdException();

        return await _checkoutService.QueueOrderAsync(request, cancellationToken);
    }
}
