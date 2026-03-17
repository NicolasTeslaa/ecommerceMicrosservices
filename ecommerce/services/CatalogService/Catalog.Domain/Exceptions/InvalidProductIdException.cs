using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class InvalidProductIdException : CatalogException
{
    public InvalidProductIdException()
        : base(CatalogErrorCode.InvalidProductId, "Product id is required.")
    {
    }
}
