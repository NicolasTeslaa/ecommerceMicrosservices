using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public class InvalidProductWeightException : CatalogException
{
    public InvalidProductWeightException()
        : base(CatalogErrorCode.InvalidProductWeight, "Product weight must be greater than zero.")
    {
    }
}
