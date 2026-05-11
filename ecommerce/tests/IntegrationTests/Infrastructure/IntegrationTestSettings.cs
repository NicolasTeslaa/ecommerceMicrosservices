namespace IntegrationTests.Infrastructure;

public sealed class IntegrationTestSettings
{
    public const string SectionName = "IntegrationTests";

    public string GatewayBaseUrl { get; set; } = "http://localhost:5100";
    public int RequestTimeoutSeconds { get; set; } = 15;
    public int ConsistencyTimeoutSeconds { get; set; } = 60;
    public int PollIntervalMilliseconds { get; set; } = 1000;
}
