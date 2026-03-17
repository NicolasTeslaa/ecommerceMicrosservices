namespace ECommerce.Shared.Contracts;

public class ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
