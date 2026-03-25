using Inventory.Application.DTOs;
using MediatR;

namespace Inventory.Application.Queries;

public record GetInventoryAvailabilityBatchQuery(IReadOnlyCollection<Guid> ProductIds) : IRequest<IReadOnlyCollection<InventoryAvailabilityDto>>;
