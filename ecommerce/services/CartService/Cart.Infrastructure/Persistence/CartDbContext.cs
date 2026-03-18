using Cart.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Persistence;

public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
    {
    }

    public DbSet<Cart.Domain.Entities.Cart> Carts => Set<Cart.Domain.Entities.Cart>();
    public DbSet<Cart.Domain.Entities.CartItem> CartItems => Set<Cart.Domain.Entities.CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CartConfiguration());
        modelBuilder.ApplyConfiguration(new CartItemConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
