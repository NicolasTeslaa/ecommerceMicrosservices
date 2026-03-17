using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class InvalidCategoryNameException : CatalogException
{
    public InvalidCategoryNameException()
        : base(CatalogErrorCode.InvalidCategoryName, "Category name is required.")
    {
    }
}
