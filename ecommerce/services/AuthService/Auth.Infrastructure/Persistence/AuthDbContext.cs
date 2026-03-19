using Auth.Domain.Entities;
using Auth.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<AuthUser> Users => Set<AuthUser>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AuthUserConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
