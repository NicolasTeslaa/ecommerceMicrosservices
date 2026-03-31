using ECommerce.Shared.Contracts;
using ECommerce.Shared.Observability;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Order.API.Common.Middleware;
using Order.API.Read.Grpc;
using Order.Application.Handlers;
using Order.Domain.Enums;
using Order.Infrastructure.Configuration;
using Order.Infrastructure.Persistence;

var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

var builder = WebApplication.CreateBuilder(args);

builder.AddECommerceObservability("order-read-api");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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

        return new BadRequestObjectResult(ApiResponse<object?>.Fail(OrderErrorCode.InvalidRequest.ToString(), message));
    };
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var readDbContext = scope.ServiceProvider.GetRequiredService<OrderReadDbContext>();
    await readDbContext.Database.MigrateAsync();

    var writeDbContext = scope.ServiceProvider.GetRequiredService<OrderWriteDbContext>();
    await writeDbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!runningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapGrpcService<OrderPaymentAccessGrpcService>();
app.MapControllers();

app.Run();
