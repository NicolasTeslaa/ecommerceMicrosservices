using ECommerce.Shared.Contracts;
using Expedition.Application.Commands;
using Expedition.Application.DTOs;
using Expedition.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Expedition.API.Controllers;

[ApiController]
[Route("api/expeditions")]
public class ExpeditionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpeditionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("orders/{orderId:guid}")]
    public async Task<ActionResult<ApiResponse<ExpeditionDto?>>> GetByOrderId(Guid orderId)
    {
        var expedition = await _mediator.Send(new GetExpeditionByOrderIdQuery(orderId));

        if (expedition is null)
            return NotFound(ApiResponse<ExpeditionDto?>.Fail("NotFound", "Expedition order was not found."));

        return Ok(ApiResponse<ExpeditionDto?>.Ok(expedition, "Expedition retrieved successfully."));
    }

    [HttpPost("orders/{orderId:guid}/pickup")]
    public async Task<ActionResult<ApiResponse<ExpeditionDto>>> MarkAsPickedUp(Guid orderId)
    {
        var expedition = await _mediator.Send(new MarkExpeditionPickedUpCommand(orderId));
        return Ok(ApiResponse<ExpeditionDto>.Ok(expedition, "Expedition marked as picked up successfully."));
    }

    [HttpPost("orders/{orderId:guid}/in-transit")]
    public async Task<ActionResult<ApiResponse<ExpeditionDto>>> MarkAsInTransit(Guid orderId)
    {
        var expedition = await _mediator.Send(new MarkExpeditionInTransitCommand(orderId));
        return Ok(ApiResponse<ExpeditionDto>.Ok(expedition, "Expedition marked as in transit successfully."));
    }

    [HttpPost("orders/{orderId:guid}/deliver")]
    public async Task<ActionResult<ApiResponse<ExpeditionDto>>> MarkAsDelivered(Guid orderId)
    {
        var expedition = await _mediator.Send(new MarkExpeditionDeliveredCommand(orderId));
        return Ok(ApiResponse<ExpeditionDto>.Ok(expedition, "Expedition marked as delivered successfully."));
    }

    [HttpPost("orders/{orderId:guid}/delivery-failure")]
    public async Task<ActionResult<ApiResponse<ExpeditionDto>>> MarkAsDeliveryFailed(
        Guid orderId,
        [FromBody] MarkDeliveryFailedRequest request)
    {
        var expedition = await _mediator.Send(
            new MarkExpeditionDeliveryFailedCommand(orderId, request.FailureReason, request.FailureDetails));

        return Ok(ApiResponse<ExpeditionDto>.Ok(expedition, "Expedition marked as delivery failed successfully."));
    }
}

public class MarkDeliveryFailedRequest
{
    public string FailureReason { get; set; } = string.Empty;
    public string? FailureDetails { get; set; }
}
