using Customer.API.Grpc;
using Customer.API.Middleware;
using Customer.Application.Handlers;
using Customer.Domain.Enums;
using Customer.Infrastructure.Configuration;
using ECommerce.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:5107", "http://localhost:5117");

Console.WriteLine("Starting Customer.API host configuration...");

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

        return new BadRequestObjectResult(ApiResponse<object?>.Fail(CustomerErrorCode.InvalidRequest.ToString(), message));
    };
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetCustomerByIdHandler).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

Console.WriteLine("Customer.API host built successfully.");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapGrpcService<CustomerAddressValidationGrpcService>();
app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation(
        "Customer API started successfully. Listening on: {Urls}",
        string.Join(", ", app.Urls));
    Console.WriteLine($"Customer API started successfully. Listening on: {string.Join(", ", app.Urls)}");
});

app.Run();
