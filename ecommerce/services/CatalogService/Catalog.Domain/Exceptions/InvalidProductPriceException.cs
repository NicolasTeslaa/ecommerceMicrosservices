using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class InvalidProductPriceException : CatalogException
{
    public InvalidProductPriceException()
        : base(CatalogErrorCode.InvalidProductPrice, "Price cannot be negative.")
    {
    }
}
