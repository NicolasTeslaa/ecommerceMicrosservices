using Catalog.Application.Commands;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Write.Controllers;

[ApiController]
[Route("api/catalog/categories")]
public class CategoriesWriteController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesWriteController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateCategoryCommand command)
    {
        var id = await _mediator.Send(command);
        var response = ApiResponse<Guid>.Ok(id, "Category created successfully.");

        return Accepted(response);
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
