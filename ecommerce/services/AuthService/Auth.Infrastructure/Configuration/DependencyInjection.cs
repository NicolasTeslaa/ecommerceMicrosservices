using Auth.Application.Interfaces;
using Auth.Infrastructure.Messaging;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetRequiredConnectionString(configuration, "AuthDb");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddDbContext<AuthDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddScoped<IAuthRegistrationService, AuthRegistrationService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddHostedService<AuthOutboxPublisherService>();

        return services;
    }

    private static string GetRequiredConnectionString(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' was not configured.");

        return connectionString;
    }
}
