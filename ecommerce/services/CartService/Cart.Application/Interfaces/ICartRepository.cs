using Cart.Domain.Enums;
using CartEntity = Cart.Domain.Entities.Cart;

namespace Cart.Application.Interfaces;

public interface ICartRepository
{
    Task<CartEntity?> GetByOwnerAsync(string ownerId, CartOwnerType ownerType, CancellationToken cancellationToken = default);
    Task AddAsync(CartEntity cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(CartEntity cart, CancellationToken cancellationToken = default);
}
