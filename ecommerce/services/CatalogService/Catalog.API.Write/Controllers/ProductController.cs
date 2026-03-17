using Catalog.Application.Commands;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Write.Controllers;

[ApiController]
[Route("api/catalog/products")]
public class ProductsWriteController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsWriteController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateProductCommand command)
    {
        var id = await _mediator.Send(command);
        var response = ApiResponse<Guid>.Ok(id, "Product created successfully.");

        return Accepted(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Guid>>> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        command.Id = id;

        var productId = await _mediator.Send(command);

        return Ok(ApiResponse<Guid>.Ok(productId, "Product updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Guid>>> Delete(Guid id)
    {
        var productId = await _mediator.Send(new DeactivateProductCommand(id));

        return Ok(ApiResponse<Guid>.Ok(productId, "Product deleted successfully."));
    }
}
