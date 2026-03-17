using Catalog.Application.DTOs;
using MediatR;

namespace Catalog.Application.Queries;

public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
{
}
