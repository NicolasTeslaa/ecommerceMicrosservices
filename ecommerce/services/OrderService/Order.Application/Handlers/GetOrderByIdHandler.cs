using MediatR;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Application.Queries;
using Order.Domain.Exceptions;

namespace Order.Application.Handlers;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderReadRepository _repository;

    public GetOrderByIdHandler(IOrderReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            throw new InvalidOrderIdException();

        var order = await _repository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
            throw new OrderNotFoundException(request.OrderId);

        return order.ToDto();
    }
}
