using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotaFiscal.API.Middleware;
using NotaFiscal.Application.Handlers;
using NotaFiscal.Infrastructure.Configuration;

var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

var builder = WebApplication.CreateBuilder(args);

if (!runningInContainer)
{
    builder.WebHost.UseUrls("https://localhost:5112", "http://localhost:5122");
}

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

        return new BadRequestObjectResult(ApiResponse<object?>.Fail("InvalidRequest", message));
    };
});
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetInvoiceByOrderIdHandler).Assembly));
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

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
