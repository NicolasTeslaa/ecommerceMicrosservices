using Expedition.Application.DTOs;
using Expedition.Application.Interfaces;
using Expedition.Application.Queries;
using MediatR;

namespace Expedition.Application.Handlers;

public class GetExpeditionByOrderIdHandler : IRequestHandler<GetExpeditionByOrderIdQuery, ExpeditionDto?>
{
    private readonly IExpeditionRepository _repository;

    public GetExpeditionByOrderIdHandler(IExpeditionRepository repository)
    {
        _repository = repository;
    }

    public Task<ExpeditionDto?> Handle(GetExpeditionByOrderIdQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
    }
}
