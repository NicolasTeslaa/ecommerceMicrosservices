using System.Text;
using Auth.API.Middleware;
using Auth.Application.Handlers;
using Auth.Domain.Enums;
using Auth.Infrastructure.Configuration;
using Auth.Infrastructure.Persistence;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddECommerceObservability("auth-api");

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

        return new BadRequestObjectResult(ApiResponse<object?>.Fail(AuthErrorCode.InvalidRequest.ToString(), message));
    };
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserHandler).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

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
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AuthStartup");

if (missingJwtSettings.Count > 0)
{
    startupLogger.LogError("Auth API started with fallback JWT settings because these keys were missing: {MissingKeys}.", string.Join(", ", missingJwtSettings));
}

await using (var scope = app.Services.CreateAsyncScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception exception)
    {
        startupLogger.LogError(exception, "Auth API failed to apply database migrations during startup.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
