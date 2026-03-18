using Cart.Application.DTOs;
using Cart.Domain.Enums;
using MediatR;

namespace Cart.Application.Commands;

public class ClearCartCommand : IRequest<CartDto>
{
    public string OwnerId { get; set; } = string.Empty;
    public CartOwnerType OwnerType { get; set; }
}
