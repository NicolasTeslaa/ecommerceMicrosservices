using Catalog.Application.Commands;
using Catalog.Application.DTOs;
using Catalog.Application.Queries;
using Catalog.API.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateProductCommand command)
    {
        var id = await _mediator.Send(command);
        var response = ApiResponse<Guid>.Ok(id, "Product created successfully.");

        return CreatedAtAction(nameof(GetById), new { id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));

        return Ok(ApiResponse<ProductDto>.Ok(product, "Product retrieved successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());

        return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(result, "Products retrieved successfully."));
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
