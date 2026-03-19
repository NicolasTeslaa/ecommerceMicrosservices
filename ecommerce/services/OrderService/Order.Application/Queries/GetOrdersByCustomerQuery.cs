using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyCollection<OrderDto>>;
