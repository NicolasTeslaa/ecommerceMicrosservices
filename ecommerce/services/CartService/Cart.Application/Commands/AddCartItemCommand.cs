using System.ComponentModel.DataAnnotations;
using Cart.Application.DTOs;
using Cart.Domain.Enums;
using MediatR;

namespace Cart.Application.Commands;

public class AddCartItemCommand : IRequest<CartDto>
{
    public string OwnerId { get; set; } = string.Empty;
    public CartOwnerType OwnerType { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
