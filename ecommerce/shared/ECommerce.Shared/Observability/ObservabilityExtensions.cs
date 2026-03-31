using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ECommerce.Shared.Observability;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddECommerceObservability(this WebApplicationBuilder builder, string serviceName)
    {
        var otlpEndpoint = ObservabilityEndpointResolver.ResolveOtlpEndpoint(builder.Configuration, builder.Environment);
        var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "dev";

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceNamespace: "ecommerce",
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation(options => options.RecordException = true);

                if (otlpEndpoint is not null)
                {
                    tracing.AddOtlpExporter(options => ConfigureExporter(options, otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (otlpEndpoint is not null)
                {
                    metrics.AddOtlpExporter(options => ConfigureExporter(options, otlpEndpoint));
                }
            });

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;

            if (otlpEndpoint is not null)
            {
                options.AddOtlpExporter(exporterOptions => ConfigureExporter(exporterOptions, otlpEndpoint));
            }
        });

        return builder;
    }

    private static void ConfigureExporter(OtlpExporterOptions options, Uri endpoint)
    {
        options.Endpoint = endpoint;
        options.Protocol = OtlpExportProtocol.Grpc;
    }
}
