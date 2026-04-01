using System.Net;
using System.Text.Json;
using ECommerce.Shared.Contracts;

namespace Expedition.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException exception)
        {
            _logger.LogWarning(exception, "Expedition resource not found.");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                ApiResponse<object?>.Fail("NotFound", exception.Message)));
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "Invalid expedition operation.");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                ApiResponse<object?>.Fail("InvalidOperation", exception.Message)));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Expedition API.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                ApiResponse<object?>.Fail("Unknown", "An unexpected error occurred while processing the expedition request.")));
        }
    }
}
