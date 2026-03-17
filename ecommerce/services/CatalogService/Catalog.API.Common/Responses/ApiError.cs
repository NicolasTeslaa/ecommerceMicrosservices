using Catalog.Domain.Enums;

namespace Catalog.API.Common.Responses;

public class ApiError
{
    public CatalogErrorCode Code { get; init; }
    public string Message { get; init; } = string.Empty;
}
