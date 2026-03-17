using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class InvalidCategoryIdException : CatalogException
{
    public InvalidCategoryIdException()
        : base(CatalogErrorCode.InvalidCategoryId, "Category id is required.")
    {
    }
}
