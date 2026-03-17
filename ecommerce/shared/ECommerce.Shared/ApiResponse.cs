namespace ECommerce.Shared.Contracts;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public PaginationMetadata? Pagination { get; init; }

    public static ApiResponse<T> Ok(T? data, string message, PaginationMetadata? pagination = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Pagination = pagination
        };
    }

    public static ApiResponse<T> Fail(string errorCode, string message)
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
