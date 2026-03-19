using Microsoft.Extensions.DependencyInjection;
using Shipping.Application.Interfaces;
using Shipping.Infrastructure.Adapters;
using Shipping.Infrastructure.Services;

namespace Shipping.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IShippingCalculator, MockShippingCalculator>();
        services.AddScoped<IShippingProviderAdapter, CorreiosAdapter>();
        services.AddScoped<IShippingProviderAdapter, MelhorEnvioAdapter>();
        services.AddScoped<IShippingProviderAdapter, UberAdapter>();
        services.AddScoped<IShippingProviderAdapter, NinetyNineAdapter>();
        services.AddScoped<IShippingProviderAdapter, PSFreteAdapter>();

        return services;
    }
}
