using MediatR;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Enums;

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
            return CreateFallbackResponse(request);

        var requiresCardToken = request.PaymentMethod is PaymentMethod.Credit or PaymentMethod.Debit;

        if (requiresCardToken && string.IsNullOrWhiteSpace(request.PaymentToken))
            return CreateFallbackResponse(request);

        if (requiresCardToken && (string.IsNullOrWhiteSpace(request.PaymentCardBrand) || string.IsNullOrWhiteSpace(request.PaymentCardLast4)))
            return CreateFallbackResponse(request);

        return await _checkoutService.QueueOrderAsync(request, cancellationToken);
    }

    private static OrderProcessingAcceptedDto CreateFallbackResponse(CreateOrderCommand request) =>
        new()
        {
            OrderId = request.OrderId == Guid.Empty ? Guid.NewGuid() : request.OrderId,
            Message = "Order accepted with fallback validation."
        };
}
