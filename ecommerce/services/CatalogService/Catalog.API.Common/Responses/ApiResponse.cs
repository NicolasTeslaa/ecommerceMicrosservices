using Catalog.Domain.Enums;

namespace Catalog.API.Common.Responses;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T? data, string message)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(CatalogErrorCode errorCode, string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = new ApiError
            {
                Code = errorCode,
                Message = message
            }
        };
    }
}
