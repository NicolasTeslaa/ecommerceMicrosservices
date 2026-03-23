using Microsoft.AspNetCore.SignalR;

namespace Payment.Tests.Support;

internal sealed class TestGroupManager : IGroupManager
{
    public List<(string ConnectionId, string GroupName)> AddedGroups { get; } = new();

    public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        AddedGroups.Add((connectionId, groupName));
        return Task.CompletedTask;
    }

    public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
