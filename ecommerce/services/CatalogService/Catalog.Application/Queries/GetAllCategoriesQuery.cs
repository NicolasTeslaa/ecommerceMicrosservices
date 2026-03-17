using Catalog.Application.DTOs;
using ECommerce.Shared.Contracts;
using MediatR;

namespace Catalog.Application.Queries;

public class GetAllCategoriesQuery : PaginationRequest, IRequest<PagedResult<CategoryDto>>
{
}
