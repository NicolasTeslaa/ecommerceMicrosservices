using Catalog.Application.Handlers;
using Catalog.API.Read.Grpc;
using Catalog.API.Common.Middleware;
using Catalog.Domain.Enums;
using Catalog.Infrastructure.Configuration;
using Catalog.Infrastructure.Persistence;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Observability;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

var builder = WebApplication.CreateBuilder(args);

builder.AddECommerceObservability("catalog-read-api");

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

        return new BadRequestObjectResult(ApiResponse<object?>.Fail(CatalogErrorCode.InvalidRequest.ToString(), message));
    };
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateProductHandler).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var readDbContext = scope.ServiceProvider.GetRequiredService<CatalogReadDbContext>();
    await readDbContext.Database.MigrateAsync();

    var writeDbContext = scope.ServiceProvider.GetRequiredService<CatalogWriteDbContext>();
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

app.MapGrpcService<CatalogProductAvailabilityGrpcService>();
app.MapControllers();

app.Run();
