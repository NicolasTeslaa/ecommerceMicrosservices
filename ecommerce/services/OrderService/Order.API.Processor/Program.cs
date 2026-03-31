using ECommerce.Shared.Observability;
using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Configuration;
using Order.Infrastructure.Persistence;

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
}

var builder = WebApplication.CreateBuilder(args);

builder.AddECommerceObservability("order-processor");

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration, enableProcessorConsumer: true);

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

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
