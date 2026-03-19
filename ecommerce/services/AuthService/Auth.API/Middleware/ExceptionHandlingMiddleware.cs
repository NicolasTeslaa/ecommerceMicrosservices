using System.Net;
using System.Text.Json;
using Auth.Domain.Enums;
using Auth.Domain.Exceptions;
using ECommerce.Shared.Contracts;

namespace Auth.API.Middleware;

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
        catch (AuthException exception)
        {
            _logger.LogWarning(exception, "Auth domain error handled: {ErrorCode}", exception.ErrorCode);
            await WriteErrorResponseAsync(context, MapStatusCode(exception), exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Auth API.");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                AuthErrorCode.Unknown,
                "An unexpected error occurred while processing the request.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        AuthErrorCode errorCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object?>.Fail(errorCode.ToString(), message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static HttpStatusCode MapStatusCode(AuthException exception)
    {
        return exception switch
        {
            InvalidEmailException => HttpStatusCode.BadRequest,
            InvalidPasswordException => HttpStatusCode.BadRequest,
            InvalidFullNameException => HttpStatusCode.BadRequest,
            UserAlreadyExistsException => HttpStatusCode.Conflict,
            InvalidCredentialsException => HttpStatusCode.Unauthorized,
            PersistenceException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.BadRequest
        };
    }
}
