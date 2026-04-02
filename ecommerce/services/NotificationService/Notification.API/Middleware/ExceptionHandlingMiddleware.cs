using System.Net;
using System.Text.Json;
using ECommerce.Shared.Contracts;

namespace Notification.API.Middleware;

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
            _logger.LogWarning(exception, "Notification resource not found.");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                ApiResponse<object?>.Fail("NotFound", exception.Message)));
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "Invalid notification operation.");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                ApiResponse<object?>.Fail("InvalidOperation", exception.Message)));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Notification API.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                ApiResponse<object?>.Fail("Unknown", "An unexpected error occurred while processing the notification request.")));
        }
    }
}
