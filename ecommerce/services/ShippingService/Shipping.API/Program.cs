using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Shipping.API.Middleware;
using Shipping.Application.Handlers;
using Shipping.Domain.Enums;
using Shipping.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .ToArray();

        var message = errors.Length > 0
            ? string.Join(" ", errors)
            : "The request payload is invalid.";

        return new BadRequestObjectResult(ApiResponse<object?>.Fail(ShippingErrorCode.InvalidRequest.ToString(), message));
    };
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CalculateShippingHandler).Assembly));
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
