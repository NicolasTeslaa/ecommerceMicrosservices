using Catalog.Application.DTOs;
using Catalog.Application.Queries;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Read.Controllers;

[ApiController]
[Route("api/catalog/products")]
public class ProductsReadController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsReadController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));

        return Ok(ApiResponse<ProductDto>.Ok(product, "Product retrieved successfully.", PaginationMetadata.SingleItem()));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ProductDto>>>> GetAll([FromQuery] GetAllProductsQuery query)
    {
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<IReadOnlyCollection<ProductDto>>.Ok(
            result.Items,
            "Products retrieved successfully.",
            result.Pagination));
    }
}
