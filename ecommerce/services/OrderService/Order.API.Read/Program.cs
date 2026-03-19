using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Order.API.Common.Middleware;
using Order.Application.Handlers;
using Order.Domain.Enums;
using Order.Infrastructure.Configuration;

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

        return new BadRequestObjectResult(ApiResponse<object?>.Fail(OrderErrorCode.InvalidRequest.ToString(), message));
    };
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

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
