using Cart.Application.DTOs;
using Cart.Domain.Enums;
using MediatR;

namespace Cart.Application.Commands;

public class RemoveCartItemCommand : IRequest<CartDto>
{
    public string OwnerId { get; set; } = string.Empty;
    public CartOwnerType OwnerType { get; set; }
    public Guid ProductId { get; set; }
}
