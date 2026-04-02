using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.DTOs;
using Notification.Application.Queries;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("orders/{orderId:guid}")]
    public async Task<ActionResult<ApiResponse<OrderNotificationsDto>>> GetByOrderId(Guid orderId)
    {
        var notifications = await _mediator.Send(new GetNotificationsByOrderIdQuery(orderId));
        return Ok(ApiResponse<OrderNotificationsDto>.Ok(notifications, "Notifications retrieved successfully."));
    }
}
