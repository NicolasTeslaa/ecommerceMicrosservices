using Catalog.Application.DTOs;
using ECommerce.Shared.Contracts;
using MediatR;

namespace Catalog.Application.Queries;

public class GetAllProductsQuery : PaginationRequest, IRequest<PagedResult<ProductDto>>
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
}
