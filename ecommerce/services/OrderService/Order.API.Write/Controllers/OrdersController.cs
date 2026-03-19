using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Commands;
using Order.Application.DTOs;

namespace Order.API.Write.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderProcessingAcceptedDto>>> Create([FromBody] CreateOrderCommand command)
    {
        var acceptedOrder = await _mediator.Send(command);
        return Accepted(ApiResponse<OrderProcessingAcceptedDto>.Ok(
            acceptedOrder,
            "Pedido recebido. Ele sera processado em instantes e voce sera notificado apos a conclusao."));
    }
}
