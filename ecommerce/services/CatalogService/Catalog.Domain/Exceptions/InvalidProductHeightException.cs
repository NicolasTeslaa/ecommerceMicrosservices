using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public class InvalidProductHeightException : CatalogException
{
    public InvalidProductHeightException()
        : base(CatalogErrorCode.InvalidProductHeight, "Product height must be greater than zero.")
    {
    }
}
