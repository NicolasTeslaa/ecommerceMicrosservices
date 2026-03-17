using Catalog.Application.DTOs;
using MediatR;

namespace Catalog.Application.Queries;

public class GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>
{
}
