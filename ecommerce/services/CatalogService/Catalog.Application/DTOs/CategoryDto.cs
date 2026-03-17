using Catalog.Domain.Entities;
using Catalog.Application.ReadModels;

namespace Catalog.Application.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static CategoryDto MapFromEntity(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public static CategoryDto MapFromReadModel(CategoryReadModel category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }
}
