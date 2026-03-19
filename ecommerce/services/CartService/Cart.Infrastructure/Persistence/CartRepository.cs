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


    // feito em sql puro pois o EF Core não tem suporte a update com join, e o update do carrinho precisa atualizar os itens relacionados
    public async Task UpdateAsync(Domain.Entities.Cart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            _context.ChangeTracker.Clear();

            var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE carts
                SET owner_id = {cart.OwnerId},
                    owner_type = {cart.OwnerType.ToString()},
                    status = {cart.Status.ToString()},
                    updated_at_utc = {cart.UpdatedAtUtc}
                WHERE Id = {cart.Id}",
                cancellationToken);

            if (affectedRows == 0)
                throw new CartNotFoundException(cart.OwnerId, cart.OwnerType);

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM cart_items WHERE cart_id = {cart.Id}",
                cancellationToken);

            foreach (var item in cart.Items)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    INSERT INTO cart_items
                    (Id, cart_id, product_id, product_name, unit_price, quantity, created_at_utc, updated_at_utc)
                    VALUES
                    ({item.Id}, {cart.Id}, {item.ProductId}, {item.ProductName}, {item.UnitPrice}, {item.Quantity}, {item.CreatedAtUtc}, {item.UpdatedAtUtc})",
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to update cart '{cart.Id}'.", exception);
        }
    }
}
