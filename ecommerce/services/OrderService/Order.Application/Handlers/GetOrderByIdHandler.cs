using MediatR;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Application.Queries;
using Order.Domain.Enums;

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
        {
            return CreateFallbackOrder(Guid.Empty);
        }

        var order = await _repository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
            return CreateFallbackOrder(request.OrderId);

        return order.ToDto();
    }

    private static OrderDto CreateFallbackOrder(Guid orderId) =>
        new()
        {
            Id = orderId,
            Status = OrderStatus.PendingPayment,
            Items = Array.Empty<OrderItemDto>(),
            RejectionDetail = "Order not available."
        };
}
