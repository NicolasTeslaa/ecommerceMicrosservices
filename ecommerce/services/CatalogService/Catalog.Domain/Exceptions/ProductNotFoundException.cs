using Catalog.Domain.Enums;

namespace Catalog.Domain.Exceptions;

public sealed class ProductNotFoundException : CatalogException
{
    public ProductNotFoundException(Guid productId)
        : base(CatalogErrorCode.ProductNotFound, $"Product '{productId}' was not found.")
    {
    }
}
