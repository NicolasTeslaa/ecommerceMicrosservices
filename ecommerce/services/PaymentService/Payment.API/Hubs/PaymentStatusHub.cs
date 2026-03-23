using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Payment.Application.Interfaces;
using Payment.Application.Queries;

namespace Payment.API.Hubs;

[Authorize]
public class PaymentStatusHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IOrderPaymentAccessClient _orderPaymentAccessClient;

    public PaymentStatusHub(IMediator mediator, IOrderPaymentAccessClient orderPaymentAccessClient)
    {
        _mediator = mediator;
        _orderPaymentAccessClient = orderPaymentAccessClient;
    }

    public async Task JoinOrderPayment(Guid orderId)
    {
        var customerIdClaim = Context.User?.FindFirstValue("customerId");

        if (!Guid.TryParse(customerIdClaim, out var authenticatedCustomerId))
            throw new HubException("Authenticated customer was not identified.");

        var payment = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId));

        if (payment is not null)
        {
            if (payment.CustomerId != authenticatedCustomerId)
                throw new HubException("Payment was not found.");
        }
        else
        {
            var access = await _orderPaymentAccessClient.ValidateAsync(orderId, authenticatedCustomerId, Context.ConnectionAborted);

            if (!access.OrderExists || !access.CustomerMatches)
                throw new HubException("Payment was not found.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, BuildOrderGroup(orderId));
    }

    public static string BuildOrderGroup(Guid orderId) => $"payment-order-{orderId:N}";
}
