using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public class InvalidProductCubageException : CatalogException
{
    public InvalidProductCubageException()
        : base(CatalogErrorCode.InvalidProductCubage, "Product cubage must be greater than zero.")
    {
    }
}
