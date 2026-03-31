using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Tests.Support;

internal sealed class TestServerCallContext : ServerCallContext
{
    private readonly Metadata _requestHeaders = new();
    private readonly Metadata _responseTrailers = new();
    private readonly Dictionary<object, object> _userState = new();
    private Status _status;
    private WriteOptions? _writeOptions;

    protected override string MethodCore => "test";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "peer";
    protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
    protected override Metadata RequestHeadersCore => _requestHeaders;
    protected override CancellationToken CancellationTokenCore => CancellationToken.None;
    protected override Metadata ResponseTrailersCore => _responseTrailers;
    protected override Status StatusCore
    {
        get => _status;
        set => _status = value;
    }

    protected override WriteOptions? WriteOptionsCore
    {
        get => _writeOptions;
        set => _writeOptions = value;
    }

    protected override AuthContext AuthContextCore =>
        new(string.Empty, new Dictionary<string, List<AuthProperty>>());

    protected override IDictionary<object, object> UserStateCore => _userState;

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
    {
        throw new NotSupportedException();
    }
}
