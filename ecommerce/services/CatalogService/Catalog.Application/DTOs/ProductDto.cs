using Catalog.Application.ReadModels;
using Catalog.Domain.Entities;

namespace Catalog.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Active { get; set; }
    public Guid CategoryId { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal CubageM3 { get; set; }
    public decimal WeightKg { get; set; }
    public string OriginZipCode { get; set; } = string.Empty;

    public static ProductDto MapFromEntity(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Active = product.Active,
            CategoryId = product.CategoryId,
            HeightCm = product.HeightCm,
            WidthCm = product.WidthCm,
            CubageM3 = product.CubageM3,
            WeightKg = product.WeightKg,
            OriginZipCode = product.OriginZipCode
        };
    }

    public static ProductDto MapFromReadModel(ProductReadModel product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Active = product.Active,
            CategoryId = product.CategoryId,
            HeightCm = product.HeightCm,
            WidthCm = product.WidthCm,
            CubageM3 = product.CubageM3,
            WeightKg = product.WeightKg,
            OriginZipCode = product.OriginZipCode
        };
    }

    public static Product MapToEntity(ProductDto dto)
    {
        return new Product(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.CategoryId,
            dto.HeightCm,
            dto.WidthCm,
            dto.CubageM3,
            dto.WeightKg,
            dto.OriginZipCode);
    }
}
