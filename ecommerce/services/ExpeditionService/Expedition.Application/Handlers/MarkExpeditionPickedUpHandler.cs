using Expedition.Application.Commands;
using Expedition.Application.DTOs;
using Expedition.Application.Interfaces;
using MediatR;

namespace Expedition.Application.Handlers;

public class MarkExpeditionPickedUpHandler : IRequestHandler<MarkExpeditionPickedUpCommand, ExpeditionDto>
{
    private readonly IExpeditionRepository _repository;
    private readonly IExpeditionEventPublisher _eventPublisher;

    public MarkExpeditionPickedUpHandler(
        IExpeditionRepository repository,
        IExpeditionEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ExpeditionDto> Handle(MarkExpeditionPickedUpCommand request, CancellationToken cancellationToken)
    {
        var expeditionOrder = await _repository.GetEntityByOrderIdAsync(request.OrderId, cancellationToken);
        if (expeditionOrder is null)
            return new ExpeditionDto { OrderId = request.OrderId };

        expeditionOrder.MarkAsPickedUp();
        await _eventPublisher.PublishStatusChangedAsync(expeditionOrder, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken)
            ?? new ExpeditionDto { OrderId = request.OrderId };
    }
}
