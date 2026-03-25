using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Application.Queries;
using MediatR;

namespace Inventory.Application.Handlers;

public class GetInventoryAvailabilityBatchHandler : IRequestHandler<GetInventoryAvailabilityBatchQuery, IReadOnlyCollection<InventoryAvailabilityDto>>
{
    private readonly IInventoryRepository _repository;

    public GetInventoryAvailabilityBatchHandler(IInventoryRepository repository)
      => _repository = repository;

    public Task<IReadOnlyCollection<InventoryAvailabilityDto>> Handle(GetInventoryAvailabilityBatchQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAvailabilityAsync(request.ProductIds, cancellationToken);
    }
}
