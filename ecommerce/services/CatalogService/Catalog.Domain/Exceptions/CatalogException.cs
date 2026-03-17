using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public abstract class CatalogException : Exception
{
    protected CatalogException(CatalogErrorCode errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public CatalogErrorCode ErrorCode { get; }
}
