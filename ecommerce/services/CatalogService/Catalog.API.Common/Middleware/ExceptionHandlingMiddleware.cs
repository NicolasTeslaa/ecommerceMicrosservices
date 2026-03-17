using System.Net;
using System.Text.Json;
using Catalog.Domain.Enums;
using Catalog.Domain.Exceptions;
using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Catalog.API.Common.Middleware;

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
        catch (CatalogException exception)
        {
            _logger.LogWarning(exception, "Catalog domain error handled: {ErrorCode}", exception.ErrorCode);
            await WriteErrorResponseAsync(context, MapStatusCode(exception), exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Catalog API.");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                CatalogErrorCode.Unknown,
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        CatalogErrorCode errorCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object?>.Fail(errorCode.ToString(), message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static HttpStatusCode MapStatusCode(CatalogException exception)
    {
        return exception switch
        {
            InvalidProductIdException => HttpStatusCode.BadRequest,
            InvalidProductNameException => HttpStatusCode.BadRequest,
            InvalidProductPriceException => HttpStatusCode.BadRequest,
            InvalidStockQuantityException => HttpStatusCode.BadRequest,
            InvalidCategoryIdException => HttpStatusCode.BadRequest,
            CategoryNotFoundException => HttpStatusCode.NotFound,
            ProductNotFoundException => HttpStatusCode.NotFound,
            PersistenceException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.BadRequest
        };
    }
}
