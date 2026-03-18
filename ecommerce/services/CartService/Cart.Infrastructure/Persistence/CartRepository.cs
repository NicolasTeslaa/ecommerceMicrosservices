using Cart.Application.Interfaces;
using Cart.Domain.Enums;
using Cart.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Persistence;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;

    public CartRepository(CartDbContext context) => _context = context;

    public async Task<Cart.Domain.Entities.Cart?> GetByOwnerAsync(string ownerId, CartOwnerType ownerType, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Carts
                .Include(cart => cart.Items)
                .FirstOrDefaultAsync(
                    cart => cart.OwnerId == ownerId && cart.OwnerType == ownerType,
                    cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to retrieve cart for owner '{ownerType}:{ownerId}'.", exception);
        }
    }

    public async Task AddAsync(Cart.Domain.Entities.Cart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to persist cart '{cart.Id}'.", exception);
        }
    }

    public async Task UpdateAsync(Cart.Domain.Entities.Cart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to update cart '{cart.Id}'.", exception);
        }
    }
}
