using Expedition.Application.Interfaces;
using Expedition.Infrastructure.Messaging;
using Expedition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Expedition.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ExpeditionDb")
            ?? throw new InvalidOperationException("Connection string 'ExpeditionDb' was not configured.");

        services.AddDbContext<ExpeditionDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IExpeditionRepository, ExpeditionRepository>();
        services.AddScoped<IExpeditionEventPublisher, KafkaExpeditionEventPublisher>();
        services.AddHostedService<InvoiceIssuedConsumerService>();
        services.AddHostedService<ExpeditionOutboxDispatcherService>();
        services.AddHostedService<ExpeditionStatusAutomationService>();

        return services;
    }
}
