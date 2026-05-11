using Cart.Application.Interfaces;
using Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cart.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionStringOrFallback(configuration, "CartDb");

        services.AddDbContext<CartDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddScoped<ICartRepository, CartRepository>();

        return services;
    }

    private static string GetConnectionStringOrFallback(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            using var loggerFactory = LoggerFactory.Create(_ => { });
            loggerFactory.CreateLogger("Cart.Infrastructure.Configuration.DependencyInjection")
                .LogError("Connection string '{ConnectionStringName}' was not configured. Using a fallback connection string.", connectionStringName);
            return $"Server=localhost;Port=3306;Database={connectionStringName.ToLowerInvariant()}_fallback;Uid=root;Pwd=root;";
        }

        return connectionString;
    }
}
