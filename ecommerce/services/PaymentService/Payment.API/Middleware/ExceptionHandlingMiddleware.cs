using System.Net;
using System.Text.Json;
using ECommerce.Shared.Contracts;
using Payment.Domain.Enums;
using Payment.Domain.Exceptions;

namespace Payment.API.Middleware;

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
        catch (PaymentException exception)
        {
            _logger.LogWarning(exception, "Payment domain error handled: {ErrorCode}", exception.ErrorCode);
            await WriteErrorResponseAsync(context, MapStatusCode(exception), exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Payment API.");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                PaymentErrorCode.Unknown,
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, PaymentErrorCode errorCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object?>.Fail(errorCode.ToString(), message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static HttpStatusCode MapStatusCode(PaymentException exception)
    {
        return exception switch
        {
            InvalidOrderIdException => HttpStatusCode.BadRequest,
            InvalidCustomerIdException => HttpStatusCode.BadRequest,
            InvalidAmountException => HttpStatusCode.BadRequest,
            InvalidCurrencyException => HttpStatusCode.BadRequest,
            InvalidPaymentMethodException => HttpStatusCode.BadRequest,
            InvalidPaymentIntentException => HttpStatusCode.BadRequest,
            PaymentNotFoundException => HttpStatusCode.NotFound,
            PersistenceException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.BadRequest
        };
    }
}
