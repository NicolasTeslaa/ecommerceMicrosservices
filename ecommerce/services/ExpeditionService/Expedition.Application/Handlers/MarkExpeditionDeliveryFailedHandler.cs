using Expedition.Application.Commands;
using Expedition.Application.DTOs;
using Expedition.Application.Interfaces;
using Expedition.Domain.Enums;
using MediatR;

namespace Expedition.Application.Handlers;

public class MarkExpeditionDeliveryFailedHandler : IRequestHandler<MarkExpeditionDeliveryFailedCommand, ExpeditionDto>
{
    private readonly IExpeditionRepository _repository;
    private readonly IExpeditionEventPublisher _eventPublisher;

    public MarkExpeditionDeliveryFailedHandler(
        IExpeditionRepository repository,
        IExpeditionEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ExpeditionDto> Handle(MarkExpeditionDeliveryFailedCommand request, CancellationToken cancellationToken)
    {
        var expeditionOrder = await _repository.GetEntityByOrderIdAsync(request.OrderId, cancellationToken);
        if (expeditionOrder is null)
            return new ExpeditionDto { OrderId = request.OrderId };

        if (!Enum.TryParse<DeliveryFailureReason>(request.FailureReason, ignoreCase: true, out var failureReason))
        {
            failureReason = DeliveryFailureReason.Other;
        }

        expeditionOrder.MarkAsDeliveryFailed(failureReason, request.FailureDetails);
        await _eventPublisher.PublishStatusChangedAsync(expeditionOrder, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken)
            ?? new ExpeditionDto { OrderId = request.OrderId };
    }
}
