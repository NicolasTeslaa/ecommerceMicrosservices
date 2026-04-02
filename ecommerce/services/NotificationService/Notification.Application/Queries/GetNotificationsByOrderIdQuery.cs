using MediatR;
using Notification.Application.DTOs;

namespace Notification.Application.Queries;

public record GetNotificationsByOrderIdQuery(Guid OrderId) : IRequest<OrderNotificationsDto>;
