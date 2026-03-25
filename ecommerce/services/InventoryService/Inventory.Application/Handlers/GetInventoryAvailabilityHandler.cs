using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Application.Queries;
using MediatR;

namespace Inventory.Application.Handlers;

public class GetInventoryAvailabilityHandler : IRequestHandler<GetInventoryAvailabilityQuery, InventoryAvailabilityDto?>
{
    private readonly IInventoryRepository _repository;

    public GetInventoryAvailabilityHandler(IInventoryRepository repository)
       => _repository = repository;

    public Task<InventoryAvailabilityDto?> Handle(GetInventoryAvailabilityQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAvailabilityAsync(request.ProductId, cancellationToken);
    }
}
