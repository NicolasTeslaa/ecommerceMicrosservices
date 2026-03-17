using Catalog.Application.DTOs;
using Catalog.Application.Queries;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Read.Controllers;

[ApiController]
[Route("api/catalog/categories")]
public class CategoriesReadController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesReadController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id)
    {
        var category = await _mediator.Send(new GetCategoryByIdQuery(id));

        return Ok(ApiResponse<CategoryDto>.Ok(category, "Category retrieved successfully.", PaginationMetadata.SingleItem()));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CategoryDto>>>> GetAll([FromQuery] GetAllCategoriesQuery query)
    {
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<IReadOnlyCollection<CategoryDto>>.Ok(
            result.Items,
            "Categories retrieved successfully.",
            result.Pagination));
    }
}
