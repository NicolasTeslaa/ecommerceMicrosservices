using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class CategoryNotFoundException : CatalogException
{
    public CategoryNotFoundException(Guid categoryId)
        : base(CatalogErrorCode.CategoryNotFound, $"Category '{categoryId}' was not found.")
    {
    }
}
