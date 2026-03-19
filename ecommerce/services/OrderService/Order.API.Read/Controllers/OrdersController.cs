using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Queries;

namespace Order.API.Read.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid orderId)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(orderId));
        return Ok(ApiResponse<OrderDto>.Ok(order, "Order retrieved successfully."));
    }

    [HttpGet("customers/{customerId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<OrderDto>>>> GetByCustomer(Guid customerId)
    {
        var orders = await _mediator.Send(new GetOrdersByCustomerQuery(customerId));
        return Ok(ApiResponse<IReadOnlyCollection<OrderDto>>.Ok(orders, "Customer orders retrieved successfully."));
    }
}
