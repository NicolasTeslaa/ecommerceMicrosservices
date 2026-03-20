using MediatR;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Enums;
using Order.Domain.Exceptions;

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
            throw new InvalidCustomerAddressIdException();

        var requiresCardToken = request.PaymentMethod is PaymentMethod.Credit or PaymentMethod.Debit;

        if (requiresCardToken && string.IsNullOrWhiteSpace(request.PaymentToken))
            throw new InvalidPaymentTokenException();

        if (requiresCardToken && (string.IsNullOrWhiteSpace(request.PaymentCardBrand) || string.IsNullOrWhiteSpace(request.PaymentCardLast4)))
            throw new InvalidPaymentCardDataException();

        return await _checkoutService.QueueOrderAsync(request, cancellationToken);
    }
}
