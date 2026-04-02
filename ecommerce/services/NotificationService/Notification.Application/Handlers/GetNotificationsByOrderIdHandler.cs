using MediatR;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Application.Queries;

namespace Notification.Application.Handlers;

public class GetNotificationsByOrderIdHandler : IRequestHandler<GetNotificationsByOrderIdQuery, OrderNotificationsDto>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsByOrderIdHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderNotificationsDto> Handle(GetNotificationsByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var emails = await _repository.GetEmailNotificationsByOrderIdAsync(request.OrderId, cancellationToken);
        var whatsAppMessages = await _repository.GetWhatsAppNotificationsByOrderIdAsync(request.OrderId, cancellationToken);

        return new OrderNotificationsDto
        {
            OrderId = request.OrderId,
            Emails = emails.Select(EmailNotificationDto.MapFromEntity).ToArray(),
            WhatsAppMessages = whatsAppMessages.Select(WhatsAppNotificationDto.MapFromEntity).ToArray()
        };
    }
}
