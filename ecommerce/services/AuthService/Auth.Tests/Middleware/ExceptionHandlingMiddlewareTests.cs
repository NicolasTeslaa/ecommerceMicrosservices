using System.Text.Json;
using Auth.API.Middleware;
using Auth.Domain.Exceptions;
using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Auth.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldKeepResponse_WhenNoExceptionOccurs()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            async httpContext =>
            {
                httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                await Task.CompletedTask;
            },
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnBadRequest_WhenValidationExceptionOccurs()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidEmailException(),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var response = await DeserializeAsync(context);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("InvalidEmail", response.Error!.Code);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnConflict_WhenUserAlreadyExistsExceptionOccurs()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new UserAlreadyExistsException("jane@example.com"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnUnauthorized_WhenInvalidCredentialsExceptionOccurs()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidCredentialsException(),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("boom"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var response = await DeserializeAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("Unknown", response.Error!.Code);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ApiResponse<object?>> DeserializeAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ApiResponse<object?>>(json)!;
    }
}
