using ECommerce.Shared.Contracts;
using MediatR;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Application.Queries;
using Order.Domain.Exceptions;

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
            throw new InvalidCustomerIdException();

        var orders = await _repository.GetByCustomerIdAsync(request.CustomerId, request, cancellationToken);
        return orders.Map(order => order.ToDto());
    }
}
