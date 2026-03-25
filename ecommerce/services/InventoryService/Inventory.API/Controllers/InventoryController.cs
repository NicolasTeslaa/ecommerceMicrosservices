using ECommerce.Shared.Contracts;
using Inventory.Application.DTOs;
using Inventory.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("products/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryAvailabilityDto?>>> GetByProductId(Guid productId)
    {
        var item = await _mediator.Send(new GetInventoryAvailabilityQuery(productId));
        return Ok(ApiResponse<InventoryAvailabilityDto?>.Ok(item, "Inventory retrieved successfully."));
    }

    [HttpPost("products/availability")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<InventoryAvailabilityDto>>>> GetBatch([FromBody] GetInventoryAvailabilityBatchRequest request)
    {
        var items = await _mediator.Send(new GetInventoryAvailabilityBatchQuery(request.ProductIds));
        return Ok(ApiResponse<IReadOnlyCollection<InventoryAvailabilityDto>>.Ok(items, "Inventory retrieved successfully."));
    }
}

public class GetInventoryAvailabilityBatchRequest
{
    public IReadOnlyCollection<Guid> ProductIds { get; set; } = Array.Empty<Guid>();
}
