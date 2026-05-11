using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Payment.Application.Interfaces;
using Payment.Application.Queries;

namespace Payment.API.Hubs;

[Authorize]
public class PaymentStatusHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IOrderPaymentAccessClient _orderPaymentAccessClient;
    private readonly ILogger<PaymentStatusHub> _logger;

    public PaymentStatusHub(
        IMediator mediator,
        IOrderPaymentAccessClient orderPaymentAccessClient,
        ILogger<PaymentStatusHub> logger)
    {
        _mediator = mediator;
        _orderPaymentAccessClient = orderPaymentAccessClient;
        _logger = logger;
    }

    public async Task JoinOrderPayment(Guid orderId)
    {
        var customerIdClaim = Context.User?.FindFirstValue("customerId");

        if (!Guid.TryParse(customerIdClaim, out var authenticatedCustomerId))
        {
            _logger.LogError("Payment hub connection {ConnectionId} does not have a valid authenticated customer.", Context.ConnectionId);
            return;
        }

        var payment = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId));

        if (payment is not null)
        {
            if (payment.CustomerId != authenticatedCustomerId)
            {
                _logger.LogError(
                    "Payment hub denied access to order {OrderId} for customer {CustomerId}.",
                    orderId,
                    authenticatedCustomerId);
                return;
            }
        }
        else
        {
            var access = await _orderPaymentAccessClient.ValidateAsync(orderId, authenticatedCustomerId, Context.ConnectionAborted);

            if (!access.OrderExists || !access.CustomerMatches)
            {
                _logger.LogError(
                    "Payment hub could not validate access to order {OrderId} for customer {CustomerId}.",
                    orderId,
                    authenticatedCustomerId);
                return;
            }
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, BuildOrderGroup(orderId));
    }

    public static string BuildOrderGroup(Guid orderId) => $"payment-order-{orderId:N}";
}
