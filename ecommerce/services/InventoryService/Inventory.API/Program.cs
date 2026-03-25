using ECommerce.Shared.Contracts;
using Inventory.API.Middleware;
using Inventory.Application.Handlers;
using Inventory.Infrastructure.Configuration;
using Inventory.Infrastructure.Grpc;
using MediatR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:5111", "http://localhost:5121");

builder.Services.AddControllers();
builder.Services.AddGrpc();
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

        return new BadRequestObjectResult(ApiResponse<object?>.Fail("InvalidRequest", message));
    };
});
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetInventoryAvailabilityHandler).Assembly));
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapGrpcService<InventoryOrderReservationGrpcService>();
app.MapControllers();
app.Run();
