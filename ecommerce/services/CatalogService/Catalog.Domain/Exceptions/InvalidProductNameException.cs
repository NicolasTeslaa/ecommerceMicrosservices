using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class InvalidProductNameException : CatalogException
{
    public InvalidProductNameException()
        : base(CatalogErrorCode.InvalidProductName, "Product name is required.")
    {
    }
}
