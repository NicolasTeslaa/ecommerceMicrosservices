using System.Security.Claims;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Application.Queries;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly IOrderPaymentAccessClient _orderPaymentAccessClient;

    public PaymentsController(
        IMediator mediator,
        IConfiguration configuration,
        IOrderPaymentAccessClient orderPaymentAccessClient)
    {
        _mediator = mediator;
        _configuration = configuration;
        _orderPaymentAccessClient = orderPaymentAccessClient;
    }

    [HttpGet("config")]
    public ActionResult<ApiResponse<object>> GetConfig()
    {
        return Ok(ApiResponse<object>.Ok(
            new
            {
                publishableKey = _configuration["Stripe:PublishableKey"] ?? string.Empty
            },
            "Payment configuration retrieved successfully."));
    }

    [HttpGet("orders/{orderId:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PaymentDto?>>> GetByOrderId(Guid orderId)
    {
        var customerIdClaim = User.FindFirstValue("customerId");

        if (!Guid.TryParse(customerIdClaim, out var authenticatedCustomerId))
            return Unauthorized(ApiResponse<PaymentDto?>.Fail("Unauthorized", "Authenticated customer was not identified."));

        var payment = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId));

        if (payment is null)
        {
            var access = await _orderPaymentAccessClient.ValidateAsync(orderId, authenticatedCustomerId);

            if (!access.OrderExists || !access.CustomerMatches)
                return NotFound(ApiResponse<PaymentDto?>.Fail("NotFound", "Payment was not found."));

            return Accepted(ApiResponse<PaymentDto?>.Ok(null, "Payment is still being prepared."));
        }

        if (payment.CustomerId != authenticatedCustomerId)
            return NotFound(ApiResponse<PaymentDto?>.Fail("NotFound", "Payment was not found."));

        return Ok(ApiResponse<PaymentDto?>.Ok(payment, "Payment retrieved successfully."));
    }
}
