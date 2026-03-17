using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class InvalidStockQuantityException : CatalogException
{
    public InvalidStockQuantityException()
        : base(CatalogErrorCode.InvalidStockQuantity, "Stock cannot be negative.")
    {
    }
}
