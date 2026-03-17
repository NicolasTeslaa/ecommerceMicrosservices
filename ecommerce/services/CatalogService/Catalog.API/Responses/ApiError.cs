using Catalog.Domain.Enums;

namespace Catalog.API.Responses;

public class ApiError
{
    public CatalogErrorCode Code { get; init; }
    public string Message { get; init; } = string.Empty;
}
