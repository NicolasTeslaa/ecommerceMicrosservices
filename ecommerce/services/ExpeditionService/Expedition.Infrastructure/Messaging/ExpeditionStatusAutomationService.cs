using Expedition.Application.Interfaces;
using Expedition.Domain.Entities;
using Expedition.Domain.Enums;
using Expedition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Expedition.Infrastructure.Messaging;

public class ExpeditionStatusAutomationService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpeditionStatusAutomationService> _logger;

    public ExpeditionStatusAutomationService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<ExpeditionStatusAutomationService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!IsAutomationEnabled())
                {
                    await Task.Delay(PollInterval, stoppingToken);
                    continue;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ExpeditionDbContext>();
                var eventPublisher = scope.ServiceProvider.GetRequiredService<IExpeditionEventPublisher>();

                var stepInterval = ResolveStepInterval();
                var cutoff = DateTime.UtcNow - stepInterval;

                var expeditions = await dbContext.ExpeditionOrders
                    .Where(item =>
                        (item.Status == ExpeditionStatus.AwaitingCarrierPickup
                        || item.Status == ExpeditionStatus.PickedUpByCarrier
                        || item.Status == ExpeditionStatus.InTransit)
                        && item.UpdatedAtUtc <= cutoff)
                    .OrderBy(item => item.UpdatedAtUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var expedition in expeditions)
                {
                    if (!TryAdvance(expedition, ResolveFailureChance()))
                        continue;

                    await eventPublisher.PublishStatusChangedAsync(expedition, stoppingToken);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Automated expedition status transition applied for order '{OrderId}'. New status: '{Status}'.",
                        expedition.OrderId,
                        expedition.Status);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while automating expedition status transitions.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    internal static bool TryAdvance(ExpeditionOrder expedition, double failureChance)
    {
        switch (expedition.Status)
        {
            case ExpeditionStatus.AwaitingCarrierPickup:
                expedition.MarkAsPickedUp();
                return true;

            case ExpeditionStatus.PickedUpByCarrier:
                expedition.MarkAsInTransit();
                return true;

            case ExpeditionStatus.InTransit:
                if (ShouldFail(failureChance))
                {
                    var (reason, details) = PickFailure();
                    expedition.MarkAsDeliveryFailed(reason, details);
                }
                else
                {
                    expedition.MarkAsDelivered();
                }

                return true;

            default:
                return false;
        }
    }

    internal static bool ShouldFail(double failureChance)
    {
        if (failureChance <= 0)
            return false;

        return Random.Shared.NextDouble() < failureChance;
    }

    internal static (DeliveryFailureReason Reason, string Details) PickFailure()
    {
        return Random.Shared.Next(0, 3) switch
        {
            0 => (DeliveryFailureReason.RecipientUnavailable, "No recipient was available at the address."),
            1 => (DeliveryFailureReason.AddressNotFound, "The destination address could not be found by the carrier."),
            _ => (DeliveryFailureReason.Other, "Carrier reported an unexpected delivery issue.")
        };
    }

    private bool IsAutomationEnabled()
    {
        var configured = _configuration["ExpeditionAutomation:Enabled"];
        return !string.Equals(configured, "false", StringComparison.OrdinalIgnoreCase);
    }

    private TimeSpan ResolveStepInterval()
    {
        var configured = _configuration["ExpeditionAutomation:StepIntervalSeconds"];
        return int.TryParse(configured, out var seconds) && seconds > 0
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.FromMinutes(1);
    }

    private double ResolveFailureChance()
    {
        var configured = _configuration["ExpeditionAutomation:FailureChance"];
        return double.TryParse(configured, out var value) && value >= 0 && value <= 1
            ? value
            : 0.2d;
    }
}
