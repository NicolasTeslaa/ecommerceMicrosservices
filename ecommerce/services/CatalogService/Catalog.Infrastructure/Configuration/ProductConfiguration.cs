using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.StockQuantity)
            .HasColumnName("stock_quantity")
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(x => x.HeightCm)
            .HasColumnName("height_cm")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.WidthCm)
            .HasColumnName("width_cm")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.CubageM3)
            .HasColumnName("cubage_m3")
            .HasPrecision(10, 4)
            .IsRequired();

        builder.Property(x => x.WeightKg)
            .HasColumnName("weight_kg")
            .HasPrecision(10, 3)
            .IsRequired();

        builder.Property(x => x.OriginZipCode)
            .HasColumnName("origin_zip_code")
            .HasMaxLength(20)
            .IsRequired();
    }
}
