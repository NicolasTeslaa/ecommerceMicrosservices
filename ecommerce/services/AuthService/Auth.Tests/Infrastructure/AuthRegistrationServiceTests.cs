using System.Text.Json;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Infrastructure.Persistence;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Auth.Tests.Support;

namespace Auth.Tests.Infrastructure;

public class AuthRegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldPersistUserAndOutboxMessage_WhenDataIsValid()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var user = AuthTestData.CreateUser();

        await service.RegisterAsync(user);

        Assert.Equal(1, await dbContext.Users.CountAsync());
        Assert.Equal(1, await dbContext.OutboxMessages.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_ShouldUseConfiguredTopic_WhenTopicIsProvided()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext, "custom.auth.topic");
        var user = AuthTestData.CreateUser();

        await service.RegisterAsync(user);

        var outbox = await dbContext.OutboxMessages.SingleAsync();
        Assert.Equal("custom.auth.topic", outbox.Topic);
    }

    [Fact]
    public async Task RegisterAsync_ShouldSerializeUserRegisteredEventIntoOutboxPayload()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var user = AuthTestData.CreateUser();

        await service.RegisterAsync(user);

        var outbox = await dbContext.OutboxMessages.SingleAsync();
        var integrationEvent = JsonSerializer.Deserialize<UserRegisteredIntegrationEvent>(outbox.Payload);

        Assert.NotNull(integrationEvent);
        Assert.Equal(user.Id, integrationEvent!.AuthUserId);
        Assert.Equal(user.CustomerId, integrationEvent.CustomerId);
        Assert.Equal(user.Email, integrationEvent.Email);
    }

    [Fact]
    public async Task RegisterAsync_ShouldStoreCustomerIdAsOutboxKey()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var user = AuthTestData.CreateUser();

        await service.RegisterAsync(user);

        var outbox = await dbContext.OutboxMessages.SingleAsync();
        Assert.Equal(user.CustomerId.ToString(), outbox.Key);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowPersistenceException_WhenContextIsDisposed()
    {
        var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var user = AuthTestData.CreateUser();
        await dbContext.DisposeAsync();

        var act = () => service.RegisterAsync(user);

        await Assert.ThrowsAsync<PersistenceException>(act);
    }

    private static AuthDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AuthDbContext(options);
    }

    private static AuthRegistrationService CreateService(AuthDbContext dbContext, string? topic = null)
    {
        var values = topic is null
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?> { ["Kafka:UserRegisteredTopic"] = topic };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new AuthRegistrationService(dbContext, configuration);
    }
}
