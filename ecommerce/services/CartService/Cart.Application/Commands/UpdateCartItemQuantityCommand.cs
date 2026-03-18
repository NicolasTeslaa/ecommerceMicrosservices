using System.ComponentModel.DataAnnotations;
using Cart.Application.DTOs;
using Cart.Domain.Enums;
using MediatR;

namespace Cart.Application.Commands;

public class UpdateCartItemQuantityCommand : IRequest<CartDto>
{
    public string OwnerId { get; set; } = string.Empty;
    public CartOwnerType OwnerType { get; set; }
    public Guid ProductId { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}
