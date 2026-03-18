using System.Net;
using System.Text.Json;
using Cart.Domain.Enums;
using Cart.Domain.Exceptions;
using ECommerce.Shared.Contracts;

namespace Cart.API.Middleware;

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
        catch (CartException exception)
        {
            _logger.LogWarning(exception, "Cart domain error handled: {ErrorCode}", exception.ErrorCode);
            await WriteErrorResponseAsync(context, MapStatusCode(exception), exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Cart API.");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                CartErrorCode.Unknown,
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        CartErrorCode errorCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object?>.Fail(errorCode.ToString(), message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static HttpStatusCode MapStatusCode(CartException exception)
    {
        return exception switch
        {
            InvalidOwnerIdException => HttpStatusCode.BadRequest,
            InvalidOwnerTypeException => HttpStatusCode.BadRequest,
            InvalidProductIdException => HttpStatusCode.BadRequest,
            InvalidProductNameException => HttpStatusCode.BadRequest,
            InvalidUnitPriceException => HttpStatusCode.BadRequest,
            InvalidQuantityException => HttpStatusCode.BadRequest,
            CartNotFoundException => HttpStatusCode.NotFound,
            CartItemNotFoundException => HttpStatusCode.NotFound,
            PersistenceException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.BadRequest
        };
    }
}
