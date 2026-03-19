using System.Net;
using System.Text.Json;
using Customer.Domain.Enums;
using Customer.Domain.Exceptions;
using ECommerce.Shared.Contracts;

namespace Customer.API.Middleware;

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
        catch (CustomerException exception)
        {
            _logger.LogWarning(exception, "Customer domain error handled: {ErrorCode}", exception.ErrorCode);
            await WriteErrorResponseAsync(context, MapStatusCode(exception), exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Customer API.");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                CustomerErrorCode.Unknown,
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        CustomerErrorCode errorCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object?>.Fail(errorCode.ToString(), message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static HttpStatusCode MapStatusCode(CustomerException exception)
    {
        return exception switch
        {
            InvalidCustomerEmailException => HttpStatusCode.BadRequest,
            InvalidCustomerNameException => HttpStatusCode.BadRequest,
            InvalidAddressLabelException => HttpStatusCode.BadRequest,
            InvalidRecipientNameException => HttpStatusCode.BadRequest,
            InvalidStreetException => HttpStatusCode.BadRequest,
            InvalidNumberException => HttpStatusCode.BadRequest,
            InvalidNeighborhoodException => HttpStatusCode.BadRequest,
            InvalidCityException => HttpStatusCode.BadRequest,
            InvalidStateException => HttpStatusCode.BadRequest,
            InvalidZipCodeException => HttpStatusCode.BadRequest,
            InvalidCountryException => HttpStatusCode.BadRequest,
            CustomerNotFoundException => HttpStatusCode.NotFound,
            CustomerAddressNotFoundException => HttpStatusCode.NotFound,
            PersistenceException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.BadRequest
        };
    }
}
