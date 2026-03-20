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
        return Ok(ApiResponse<OrderDto>.Ok(order, "Order retrieved successfully.", PaginationMetadata.SingleItem()));
    }

    [HttpGet("customers/{customerId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<OrderDto>>>> GetByCustomer(
        Guid customerId,
        [FromQuery] GetOrdersByCustomerQuery query)
    {
        var result = await _mediator.Send(new GetOrdersByCustomerQuery
        {
            CustomerId = customerId,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        });

        return Ok(ApiResponse<IReadOnlyCollection<OrderDto>>.Ok(
            result.Items,
            "Customer orders retrieved successfully.",
            result.Pagination));
    }
}
