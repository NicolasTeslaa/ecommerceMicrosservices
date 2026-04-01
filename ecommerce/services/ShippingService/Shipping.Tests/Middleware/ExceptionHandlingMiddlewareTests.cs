using System.Text.Json;
using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Shipping.API.Middleware;
using Shipping.Domain.Exceptions;

namespace Shipping.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldKeepResponseUntouched_WhenNoExceptionIsThrown()
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
    public async Task InvokeAsync_ShouldReturnBadRequest_WhenShippingExceptionIsThrown()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidHeightException(),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var response = await DeserializeResponseAsync(context);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.False(response.Success);
        Assert.Equal("InvalidHeight", response.Error!.Code);
        Assert.Equal("Height must be greater than zero.", response.Error.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnInternalServerError_WhenUnexpectedExceptionIsThrown()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("boom"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var response = await DeserializeResponseAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.False(response.Success);
        Assert.Equal("Unknown", response.Error!.Code);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ApiResponse<object?>> DeserializeResponseAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ApiResponse<object?>>(json)!;
    }
}
