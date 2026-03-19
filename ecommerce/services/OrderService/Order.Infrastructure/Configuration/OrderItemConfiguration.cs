using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Configuration;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.OrderId).IsRequired();
        builder.Property(item => item.ProductId).IsRequired();
        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Quantity).IsRequired();
        builder.Property(item => item.TotalPrice).HasPrecision(18, 2).IsRequired();
    }
}
