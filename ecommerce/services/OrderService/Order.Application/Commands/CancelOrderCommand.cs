using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Commands;

public class CancelOrderCommand : IRequest<OrderActionResultDto>
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
}
