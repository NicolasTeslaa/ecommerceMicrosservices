using System.Text.Json;
using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Payment.API.Middleware;
using Payment.Domain.Exceptions;

namespace Payment.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldWriteBadRequest_WhenPaymentExceptionIsThrown()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidAmountException(),
            Mock.Of<ILogger<ExceptionHandlingMiddleware>>());

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldWriteNotFound_WhenPaymentNotFoundIsThrown()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new PaymentNotFoundException(Guid.NewGuid()),
            Mock.Of<ILogger<ExceptionHandlingMiddleware>>());

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldWriteInternalServerError_WhenUnhandledExceptionIsThrown()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("boom"),
            Mock.Of<ILogger<ExceptionHandlingMiddleware>>());

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var payload = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<object?>>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.Equal("Unknown", response!.Error!.Code);
    }
}
