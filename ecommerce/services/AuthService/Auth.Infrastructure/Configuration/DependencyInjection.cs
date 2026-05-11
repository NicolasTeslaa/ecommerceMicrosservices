using Auth.Application.Interfaces;
using Auth.Infrastructure.Messaging;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionStringOrFallback(configuration, "AuthDb");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddDbContext<AuthDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddScoped<IAuthRegistrationService, AuthRegistrationService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddHostedService<AuthOutboxPublisherService>();

        return services;
    }

    private static string GetConnectionStringOrFallback(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            using var loggerFactory = LoggerFactory.Create(_ => { });
            loggerFactory.CreateLogger("Auth.Infrastructure.Configuration.DependencyInjection")
                .LogError("Connection string '{ConnectionStringName}' was not configured. Using a fallback connection string.", connectionStringName);
            return $"Server=localhost;Port=3306;Database={connectionStringName.ToLowerInvariant()}_fallback;Uid=root;Pwd=root;";
        }

        return connectionString;
    }
}
