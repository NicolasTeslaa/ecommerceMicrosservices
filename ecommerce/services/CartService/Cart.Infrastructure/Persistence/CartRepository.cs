using Cart.Application.Interfaces;
using Cart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cart.Infrastructure.Persistence;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;
    private readonly ILogger<CartRepository> _logger;

    public CartRepository(CartDbContext context, ILogger<CartRepository>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<CartRepository>.Instance;
    }

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
            _logger.LogError(exception, "Failed to retrieve cart for owner '{OwnerType}:{OwnerId}'.", ownerType, ownerId);
            return null;
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
            _logger.LogError(exception, "Failed to persist cart '{CartId}'.", cart.Id);
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
            {
                _logger.LogError("Failed to update cart '{CartId}' because it was not found for owner '{OwnerType}:{OwnerId}'.", cart.Id, cart.OwnerType, cart.OwnerId);
                return;
            }

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
            _logger.LogError(exception, "Failed to update cart '{CartId}'.", cart.Id);
        }
    }
}

