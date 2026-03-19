using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public class InvalidProductOriginZipCodeException : CatalogException
{
    public InvalidProductOriginZipCodeException()
        : base(CatalogErrorCode.InvalidProductOriginZipCode, "Product origin zip code is required.")
    {
    }
}
