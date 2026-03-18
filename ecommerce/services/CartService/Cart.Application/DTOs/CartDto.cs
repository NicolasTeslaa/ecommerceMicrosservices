using Cart.Domain.Enums;

namespace Cart.Application.DTOs;

public class CartDto
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public CartOwnerType OwnerType { get; set; }
    public CartStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public IReadOnlyCollection<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();

    public static CartDto MapFromEntity(Cart.Domain.Entities.Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            OwnerId = cart.OwnerId,
            OwnerType = cart.OwnerType,
            Status = cart.Status,
            CreatedAtUtc = cart.CreatedAtUtc,
            UpdatedAtUtc = cart.UpdatedAtUtc,
            TotalAmount = cart.TotalAmount,
            Items = cart.Items
                .OrderBy(item => item.ProductName)
                .Select(CartItemDto.MapFromEntity)
                .ToArray()
        };
    }

    public static CartDto Empty(string ownerId, CartOwnerType ownerType)
    {
        return new CartDto
        {
            Id = Guid.Empty,
            OwnerId = ownerId,
            OwnerType = ownerType,
            Status = CartStatus.Active,
            CreatedAtUtc = DateTime.MinValue,
            UpdatedAtUtc = DateTime.MinValue,
            TotalAmount = 0,
            Items = Array.Empty<CartItemDto>()
        };
    }
}
