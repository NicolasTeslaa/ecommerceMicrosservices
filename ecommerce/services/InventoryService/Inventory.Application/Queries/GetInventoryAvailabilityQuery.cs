using Inventory.Application.DTOs;
using MediatR;

namespace Inventory.Application.Queries;

public record GetInventoryAvailabilityQuery(Guid ProductId) : IRequest<InventoryAvailabilityDto?>;
