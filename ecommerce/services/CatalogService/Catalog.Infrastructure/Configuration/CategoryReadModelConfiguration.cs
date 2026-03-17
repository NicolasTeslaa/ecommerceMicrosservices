using Catalog.Application.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Configuration;

public class CategoryReadModelConfiguration : IEntityTypeConfiguration<CategoryReadModel>
{
    public void Configure(EntityTypeBuilder<CategoryReadModel> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();
    }
}
