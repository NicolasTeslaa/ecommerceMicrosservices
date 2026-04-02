using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Persistence;

namespace Notification.Tests.Infrastructure;

public class NotificationDbContextModelTests
{
    [Fact]
    public void Model_ShouldConfigureEmailNotificationEntity()
    {
        using var context = CreateDbContext();
        var entity = context.Model.FindEntityType(typeof(Notification.Domain.Entities.EmailNotification));

        Assert.NotNull(entity);
        Assert.Equal("EmailNotifications", entity!.GetTableName());
    }

    [Fact]
    public void Model_ShouldConfigureWhatsAppNotificationEntity()
    {
        using var context = CreateDbContext();
        var entity = context.Model.FindEntityType(typeof(Notification.Domain.Entities.WhatsAppNotification));

        Assert.NotNull(entity);
        Assert.Equal("WhatsAppNotifications", entity!.GetTableName());
    }

    [Fact]
    public void Model_ShouldConfigureProcessedKafkaMessageEntity()
    {
        using var context = CreateDbContext();
        var entity = context.Model.FindEntityType(typeof(Notification.Domain.Entities.ProcessedKafkaMessage));

        Assert.NotNull(entity);
        Assert.Equal("ProcessedKafkaMessages", entity!.GetTableName());
    }

    private static NotificationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NotificationDbContext(options);
    }
}
