using Catalog.Application.DTOs;
using MediatR;

namespace Catalog.Application.Queries;

public class GetCategoryByIdQuery : IRequest<CategoryDto?>
{
    public Guid Id { get; set; }

    public GetCategoryByIdQuery(Guid id)
    {
        Id = id;
    }
}
