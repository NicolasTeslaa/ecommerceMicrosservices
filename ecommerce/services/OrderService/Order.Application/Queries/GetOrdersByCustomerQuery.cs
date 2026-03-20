using ECommerce.Shared.Contracts;
using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public class GetOrdersByCustomerQuery : PaginationRequest, IRequest<PagedResult<OrderDto>>
{
    public Guid CustomerId { get; init; }
}
