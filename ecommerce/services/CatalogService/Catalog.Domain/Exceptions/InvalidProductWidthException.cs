using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public class InvalidProductWidthException : CatalogException
{
    public InvalidProductWidthException()
        : base(CatalogErrorCode.InvalidProductWidth, "Product width must be greater than zero.")
    {
    }
}
