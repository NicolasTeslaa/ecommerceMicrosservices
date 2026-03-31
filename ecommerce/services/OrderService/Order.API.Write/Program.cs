using ECommerce.Shared.Contracts;
using ECommerce.Shared.Observability;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Order.API.Common.Middleware;
using Order.Application.Handlers;
using Order.Domain.Enums;
using Order.Infrastructure.Configuration;
using Order.Infrastructure.Persistence;

var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

if (runningInContainer)
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddECommerceObservability("order-write-api");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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

builder.Services.AddInfrastructure(builder.Configuration, enableOutboxDispatcher: true);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var writeDbContext = scope.ServiceProvider.GetRequiredService<OrderWriteDbContext>();
    await writeDbContext.Database.MigrateAsync();

    var readDbContext = scope.ServiceProvider.GetRequiredService<OrderReadDbContext>();
    await readDbContext.Database.MigrateAsync();
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
app.MapControllers();

app.Run();
