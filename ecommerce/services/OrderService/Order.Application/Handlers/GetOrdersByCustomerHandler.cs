using ECommerce.Shared.Contracts;
using MediatR;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Application.Queries;

namespace Order.Application.Handlers;

public class GetOrdersByCustomerHandler : IRequestHandler<GetOrdersByCustomerQuery, PagedResult<OrderDto>>
{
    private readonly IOrderReadRepository _repository;

    public GetOrdersByCustomerHandler(IOrderReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        if (request.CustomerId == Guid.Empty)
            return PagedResult<OrderDto>.Create(Array.Empty<OrderDto>(), request.PageNumber, request.PageSize, 0);

        var orders = await _repository.GetByCustomerIdAsync(request.CustomerId, request, cancellationToken);
        return orders.Map(order => order.ToDto());
    }
}
