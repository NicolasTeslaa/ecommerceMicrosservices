using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order.Domain.Entities.Order>
{
    public void Configure(EntityTypeBuilder<Order.Domain.Entities.Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(order => order.Id);
        builder.Property(order => order.CustomerId).IsRequired();
        builder.Property(order => order.CustomerAddressId).IsRequired();
        builder.Property(order => order.CustomerEmail).HasMaxLength(200).IsRequired();
        builder.Property(order => order.ShippingAddress).HasMaxLength(400).IsRequired();
        builder.Property(order => order.ShippingAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.PaymentMethod).IsRequired();
        builder.Property(order => order.PaymentToken).HasMaxLength(200);
        builder.Property(order => order.PaymentCardBrand).HasMaxLength(50);
        builder.Property(order => order.PaymentCardLast4).HasMaxLength(4);
        builder.Property(order => order.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.Status).IsRequired();
        builder.Property(order => order.RejectionDetail).HasMaxLength(500);
        builder.Property(order => order.CreatedAtUtc).IsRequired();

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
