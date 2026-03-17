using Catalog.Domain.Entities;
using System;
using Catalog.Application.ReadModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool Active { get; set; }
    public Guid CategoryId { get; set; }

    public static ProductDto MapFromEntity(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Active = product.Active,
            CategoryId = product.CategoryId
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
            StockQuantity = product.StockQuantity,
            Active = product.Active,
            CategoryId = product.CategoryId
        };
    }

    public static Product MapToEntity(ProductDto dto)
    {
        return new Product(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.StockQuantity,
            dto.CategoryId
        );
    }
}
