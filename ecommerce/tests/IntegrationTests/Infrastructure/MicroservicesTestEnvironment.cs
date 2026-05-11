using System.Net;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests.Infrastructure;

public sealed class MicroservicesTestEnvironment : IAsyncLifetime
{
    public IntegrationTestSettings Settings { get; private set; } = new();
    public HttpClient GatewayClient { get; private set; } = null!;
    public GatewayApiClient GatewayApi { get; private set; } = null!;

    public Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        Settings = configuration
            .GetSection(IntegrationTestSettings.SectionName)
            .Get<IntegrationTestSettings>()
            ?? new IntegrationTestSettings();

        GatewayClient = new HttpClient
        {
            BaseAddress = new Uri(Settings.GatewayBaseUrl, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(Settings.RequestTimeoutSeconds)
        };
        GatewayApi = new GatewayApiClient(GatewayClient);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (GatewayClient is not null)
            GatewayClient.Dispose();

        await Task.CompletedTask;
    }

    public async Task WaitForSuccessfulResponseAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> action,
        CancellationToken cancellationToken = default)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(Settings.ConsistencyTimeoutSeconds);
        HttpStatusCode? lastStatusCode = null;
        string? lastBody = null;

        while (DateTime.UtcNow < timeoutAt)
        {
            using var response = await action(cancellationToken);
            lastStatusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
                return;

            lastBody = await response.Content.ReadAsStringAsync(cancellationToken);
            await Task.Delay(Settings.PollIntervalMilliseconds, cancellationToken);
        }

        throw new TimeoutException(
            $"Timed out after {Settings.ConsistencyTimeoutSeconds}s waiting for a successful response. " +
            $"Last status: {(int?)lastStatusCode} {lastStatusCode}. Last body: {lastBody}");
    }

    public async Task<T> WaitForAsync<T>(
        Func<CancellationToken, Task<T>> action,
        Func<T, bool> predicate,
        Func<T, string>? describe = null,
        CancellationToken cancellationToken = default)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(Settings.ConsistencyTimeoutSeconds);
        T? lastValue = default;

        while (DateTime.UtcNow < timeoutAt)
        {
            lastValue = await action(cancellationToken);

            if (predicate(lastValue))
                return lastValue;

            await Task.Delay(Settings.PollIntervalMilliseconds, cancellationToken);
        }

        throw new TimeoutException(
            $"Timed out after {Settings.ConsistencyTimeoutSeconds}s waiting for the expected condition. " +
            $"Last value: {describe?.Invoke(lastValue!) ?? lastValue?.ToString() ?? "<null>"}");
    }
}
