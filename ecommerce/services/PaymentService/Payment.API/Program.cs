using System.Text;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Payment.API.Hubs;
using Payment.API.Middleware;
using Payment.Application.Handlers;
using Payment.Application.Interfaces;
using Payment.Domain.Enums;
using Payment.Infrastructure.Configuration;
using Payment.Infrastructure.Persistence;

var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

if (runningInContainer)
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
}

var builder = WebApplication.CreateBuilder(args);

if (!runningInContainer)
{
    builder.WebHost.UseUrls("https://localhost:5110", "http://localhost:5120");
}

builder.AddECommerceObservability("payment-api");

builder.Services.AddControllers();
builder.Services.AddSignalR();
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

        return new BadRequestObjectResult(ApiResponse<object?>.Fail(PaymentErrorCode.InvalidRequest.ToString(), message));
    };
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetPaymentByOrderIdHandler).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IPaymentRealtimeNotifier, SignalRPaymentRealtimeNotifier>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"];
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var missingJwtSettings = new List<string>();

if (string.IsNullOrWhiteSpace(secretKey))
{
    missingJwtSettings.Add("Jwt:SecretKey");
    secretKey = "development-fallback-secret-key-change-me-1234567890";
}

if (string.IsNullOrWhiteSpace(issuer))
{
    missingJwtSettings.Add("Jwt:Issuer");
    issuer = "missing-issuer";
}

if (string.IsNullOrWhiteSpace(audience))
{
    missingJwtSettings.Add("Jwt:Audience");
    audience = "missing-audience";
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs/payments"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("PaymentStartup");

if (missingJwtSettings.Count > 0)
{
    startupLogger.LogError("Payment API started with fallback JWT settings because these keys were missing: {MissingKeys}.", string.Join(", ", missingJwtSettings));
}

await using (var scope = app.Services.CreateAsyncScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception exception)
    {
        startupLogger.LogError(exception, "Payment API failed to apply database migrations during startup.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<PaymentStatusHub>("/hubs/payments");
app.MapControllers();

app.Run();
