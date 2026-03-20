using System.Net;
using System.Text.Json;
using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Order.Domain.Enums;
using Order.Domain.Exceptions;

namespace Order.API.Common.Middleware;

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
        catch (OrderException exception)
        {
            _logger.LogWarning(exception, "Order domain error handled: {ErrorCode}", exception.ErrorCode);
            await WriteErrorResponseAsync(context, MapStatusCode(exception), exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Order API.");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                OrderErrorCode.Unknown,
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        OrderErrorCode errorCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var response = ApiResponse<object?>.Fail(errorCode.ToString(), message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static HttpStatusCode MapStatusCode(OrderException exception)
    {
        return exception switch
        {
            InvalidOrderIdException => HttpStatusCode.BadRequest,
            InvalidCustomerIdException => HttpStatusCode.BadRequest,
            InvalidCustomerEmailException => HttpStatusCode.BadRequest,
            InvalidCustomerAddressIdException => HttpStatusCode.BadRequest,
            InvalidShippingAddressException => HttpStatusCode.BadRequest,
            InvalidPaymentMethodException => HttpStatusCode.BadRequest,
            InvalidPaymentTokenException => HttpStatusCode.BadRequest,
            InvalidPaymentCardDataException => HttpStatusCode.BadRequest,
            InvalidOrderItemException => HttpStatusCode.BadRequest,
            InvalidProductIdException => HttpStatusCode.BadRequest,
            InvalidProductNameException => HttpStatusCode.BadRequest,
            InvalidUnitPriceException => HttpStatusCode.BadRequest,
            InvalidQuantityException => HttpStatusCode.BadRequest,
            OrderNotFoundException => HttpStatusCode.NotFound,
            CustomerAddressNotFoundException => HttpStatusCode.NotFound,
            PersistenceException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.BadRequest
        };
    }
}
