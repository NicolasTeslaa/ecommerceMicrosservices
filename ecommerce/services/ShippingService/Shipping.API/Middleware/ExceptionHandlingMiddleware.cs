using System.Net;
using System.Text.Json;
using ECommerce.Shared.Contracts;
using Shipping.Domain.Enums;
using Shipping.Domain.Exceptions;

namespace Shipping.API.Middleware;

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
        catch (ShippingException exception)
        {
            _logger.LogWarning(exception, "Shipping domain error handled: {ErrorCode}", exception.ErrorCode);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Shipping API.");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                ShippingErrorCode.Unknown,
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, ShippingErrorCode errorCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(ApiResponse<object?>.Fail(errorCode.ToString(), message)));
    }
}
