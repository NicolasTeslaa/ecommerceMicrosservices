using System.Text.Json;
using Confluent.Kafka;
using Customer.Application.Interfaces;
using Customer.Domain.Entities;
using Customer.Infrastructure.Persistence;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure.Messaging;

public class UserRegisteredConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRegisteredConsumerService> _logger;

    public UserRegisteredConsumerService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<UserRegisteredConsumerService> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers was not configured for CustomerService.");

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = _configuration["Kafka:GroupId"] ?? "customer-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = true,
            EnableAutoCommit = false
        };

        var topic = _configuration["Kafka:UserRegisteredTopic"] ?? "auth.user-registered";
        var consumerGroup = consumerConfig.GroupId;

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(topic);
        _logger.LogInformation(
            "CustomerService Kafka consumer subscribed to topic '{Topic}' on '{BootstrapServers}'.",
            topic,
            bootstrapServers);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);

                if (string.IsNullOrWhiteSpace(result.Message.Value))
                    continue;

                var integrationEvent = JsonSerializer.Deserialize<UserRegisteredIntegrationEvent>(result.Message.Value);

                if (integrationEvent is null)
                    continue;

                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
                var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

                var alreadyProcessed = await dbContext.ProcessedKafkaMessages.AnyAsync(
                    message => message.Topic == result.Topic
                        && message.Partition == result.Partition.Value
                        && message.Offset == result.Offset.Value,
                    stoppingToken);

                if (alreadyProcessed)
                {
                    consumer.Commit(result);
                    continue;
                }

                await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                var existingCustomer = await repository.GetByIdAsync(integrationEvent.CustomerId, stoppingToken);

                if (existingCustomer is null)
                {
                    var customer = new Customer.Domain.Entities.Customer(
                        integrationEvent.CustomerId,
                        integrationEvent.AuthUserId,
                        integrationEvent.FullName,
                        integrationEvent.Email,
                        integrationEvent.PhoneNumber,
                        integrationEvent.RegisteredAtUtc);

                    await repository.AddAsync(customer, stoppingToken);
                    _logger.LogInformation(
                        "Customer '{CustomerId}' created from auth registration event for '{Email}'.",
                        customer.Id,
                        customer.Email);
                }

                await dbContext.ProcessedKafkaMessages.AddAsync(
                    new ProcessedKafkaMessage(
                        result.Topic,
                        result.Partition.Value,
                        result.Offset.Value,
                        consumerGroup,
                        result.Message.Key ?? string.Empty,
                        nameof(UserRegisteredIntegrationEvent)),
                    stoppingToken);

                await dbContext.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
                consumer.Commit(result);

                _logger.LogInformation(
                    "Kafka message '{Topic}/{Partition}/{Offset}' processed successfully by CustomerService.",
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException exception)
            {
                _logger.LogError(exception, "Kafka consume error in CustomerService consumer.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error consuming user registered event.");
            }
        }
    }
}
