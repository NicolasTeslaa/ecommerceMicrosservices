using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class PersistenceException : CatalogException
{
    public PersistenceException(string message, Exception? innerException = null)
        : base(CatalogErrorCode.PersistenceFailure, message, innerException)
    {
    }
}
