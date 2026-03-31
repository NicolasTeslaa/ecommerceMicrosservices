using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Infrastructure.Messaging;
using NotaFiscal.Infrastructure.Persistence;

namespace NotaFiscal.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NotaFiscalDb")
            ?? throw new InvalidOperationException("Connection string 'NotaFiscalDb' was not configured.");

        services.AddDbContext<NotaFiscalDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceEventPublisher, KafkaInvoiceEventPublisher>();
        services.AddSingleton<IMockInvoiceFactory, MockInvoiceFactory>();
        services.AddScoped<OrderConfirmedMessageProcessor>();
        services.AddHostedService<OrderConfirmedConsumerService>();

        return services;
    }
}
