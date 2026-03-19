using Customer.Application.Interfaces;
using Customer.Infrastructure.Messaging;
using Customer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Customer.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetRequiredConnectionString(configuration, "CustomerDb");

        services.AddDbContext<CustomerDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddHostedService<UserRegisteredConsumerService>();

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
