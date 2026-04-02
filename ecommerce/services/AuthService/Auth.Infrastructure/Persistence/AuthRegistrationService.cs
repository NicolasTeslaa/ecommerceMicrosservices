using System.Text.Json;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Persistence;

public class AuthRegistrationService : IAuthRegistrationService
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthRegistrationService(AuthDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task RegisterAsync(AuthUser user, string phoneNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var integrationEvent = new UserRegisteredIntegrationEvent
            {
                AuthUserId = user.Id,
                CustomerId = user.CustomerId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = phoneNumber.Trim(),
                RegisteredAtUtc = user.CreatedAtUtc
            };

            var topic = _configuration["Kafka:UserRegisteredTopic"] ?? "auth.user-registered";
            var payload = JsonSerializer.Serialize(integrationEvent);
            var outboxMessage = OutboxMessage.Create(
                topic,
                user.CustomerId.ToString(),
                nameof(UserRegisteredIntegrationEvent),
                payload);

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to persist auth user '{user.Email}' and enqueue outbox message.", exception);
        }
    }
}
