using Catalog.API.Responses;
using Catalog.Application.Commands;
using Catalog.Application.DTOs;
using Catalog.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateCategoryCommand command)
    {
        var id = await _mediator.Send(command);
        var response = ApiResponse<Guid>.Ok(id, "Category created successfully.");

        return CreatedAtAction(nameof(GetById), new { id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id)
    {
        var category = await _mediator.Send(new GetCategoryByIdQuery(id));

        return Ok(ApiResponse<CategoryDto>.Ok(category, "Category retrieved successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery());

        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(result, "Categories retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Guid>>> Update(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        command.Id = id;

        var categoryId = await _mediator.Send(command);

        return Ok(ApiResponse<Guid>.Ok(categoryId, "Category updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Guid>>> Delete(Guid id)
    {
        var categoryId = await _mediator.Send(new DeleteCategoryCommand(id));

        return Ok(ApiResponse<Guid>.Ok(categoryId, "Category deleted successfully."));
    }
}
