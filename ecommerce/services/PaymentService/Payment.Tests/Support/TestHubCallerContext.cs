using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace Payment.Tests.Support;

internal sealed class TestHubCallerContext : HubCallerContext
{
    private readonly ClaimsPrincipal _user;

    public TestHubCallerContext(ClaimsPrincipal user)
    {
        _user = user;
        ConnectionId = Guid.NewGuid().ToString("N");
        ConnectionAborted = CancellationToken.None;
    }

    public override string ConnectionId { get; }
    public override string? UserIdentifier => _user.FindFirstValue("customerId");
    public override ClaimsPrincipal? User => _user;
    public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
    public override IFeatureCollection Features => new FeatureCollection();
    public override CancellationToken ConnectionAborted { get; }

    public override void Abort()
    {
    }
}
