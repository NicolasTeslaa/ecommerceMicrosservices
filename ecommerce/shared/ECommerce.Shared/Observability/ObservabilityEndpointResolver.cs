using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Shared.Observability;

internal static class ObservabilityEndpointResolver
{
    internal static Uri? ResolveOtlpEndpoint(IConfiguration configuration, IHostEnvironment environment)
    {
        var endpoint =
            configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ??
            configuration["Observability:Otlp:Endpoint"];

        if (string.IsNullOrWhiteSpace(endpoint) && environment.IsDevelopment())
        {
            endpoint = "http://localhost:4317";
        }

        return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ? uri : null;
    }
}
