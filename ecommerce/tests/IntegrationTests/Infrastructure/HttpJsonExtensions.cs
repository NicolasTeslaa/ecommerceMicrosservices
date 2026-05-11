using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntegrationTests.Infrastructure;

internal static class HttpJsonExtensions
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static Task<T?> ReadJsonAsync<T>(this HttpContent content, CancellationToken cancellationToken = default)
    {
        return content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }
}
