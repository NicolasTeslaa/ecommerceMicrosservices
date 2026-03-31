using ECommerce.Shared.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Shared.Tests.Observability;

public class ObservabilityEndpointResolverTests
{
    [Fact]
    public void ResolveOtlpEndpoint_ShouldPreferEnvironmentVariable()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4317",
                ["Observability:Otlp:Endpoint"] = "http://fallback:4317"
            })
            .Build();

        var endpoint = ObservabilityEndpointResolver.ResolveOtlpEndpoint(
            configuration,
            new FakeHostEnvironment(Environments.Production));

        Assert.Equal("http://collector:4317/", endpoint?.ToString());
    }

    [Fact]
    public void ResolveOtlpEndpoint_ShouldUseObservabilitySection_WhenEnvironmentVariableIsMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Observability:Otlp:Endpoint"] = "http://aspire-dashboard:18889"
            })
            .Build();

        var endpoint = ObservabilityEndpointResolver.ResolveOtlpEndpoint(
            configuration,
            new FakeHostEnvironment(Environments.Production));

        Assert.Equal("http://aspire-dashboard:18889/", endpoint?.ToString());
    }

    [Fact]
    public void ResolveOtlpEndpoint_ShouldFallbackToLocalhostInDevelopment_WhenEndpointIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var endpoint = ObservabilityEndpointResolver.ResolveOtlpEndpoint(
            configuration,
            new FakeHostEnvironment(Environments.Development));

        Assert.Equal("http://localhost:4317/", endpoint?.ToString());
    }

    [Fact]
    public void ResolveOtlpEndpoint_ShouldReturnNull_WhenEndpointIsInvalidOutsideDevelopment()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "not-a-uri"
            })
            .Build();

        var endpoint = ObservabilityEndpointResolver.ResolveOtlpEndpoint(
            configuration,
            new FakeHostEnvironment(Environments.Production));

        Assert.Null(endpoint);
    }

    [Fact]
    public void AddECommerceObservability_ShouldRegisterOpenTelemetryServices()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.AddECommerceObservability("shared-tests");

        using var provider = builder.Services.BuildServiceProvider();

        Assert.Contains(
            builder.Services,
            descriptor => descriptor.ServiceType.FullName?.Contains("TracerProvider", StringComparison.Ordinal) == true);
        Assert.Contains(
            builder.Services,
            descriptor => descriptor.ServiceType.FullName?.Contains("MeterProvider", StringComparison.Ordinal) == true);
        Assert.NotNull(provider.GetRequiredService<ILoggerFactory>());
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "ECommerce.Shared.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
