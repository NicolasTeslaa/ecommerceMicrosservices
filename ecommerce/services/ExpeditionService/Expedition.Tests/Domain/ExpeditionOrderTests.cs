using Expedition.Domain.Entities;
using Expedition.Domain.Enums;
using Xunit;

namespace Expedition.Tests.Domain;

public class ExpeditionOrderTests
{
    [Fact]
    public void Constructor_ShouldStartAwaitingCarrierPickup()
    {
        var expedition = CreateExpedition();

        Assert.Equal(ExpeditionStatus.AwaitingCarrierPickup, expedition.Status);
        Assert.Equal(DeliveryFailureReason.None, expedition.FailureReason);
    }

    [Fact]
    public void MarkAsPickedUp_ShouldAdvanceStatus()
    {
        var expedition = CreateExpedition();

        expedition.MarkAsPickedUp();

        Assert.Equal(ExpeditionStatus.PickedUpByCarrier, expedition.Status);
        Assert.NotNull(expedition.PickedUpAtUtc);
    }

    [Fact]
    public void MarkAsInTransit_ShouldAdvanceStatus_WhenPickedUp()
    {
        var expedition = CreateExpedition();
        expedition.MarkAsPickedUp();

        expedition.MarkAsInTransit();

        Assert.Equal(ExpeditionStatus.InTransit, expedition.Status);
        Assert.NotNull(expedition.InTransitAtUtc);
    }

    [Fact]
    public void MarkAsDelivered_ShouldAdvanceStatus_WhenInTransit()
    {
        var expedition = CreateExpedition();
        expedition.MarkAsPickedUp();
        expedition.MarkAsInTransit();

        expedition.MarkAsDelivered();

        Assert.Equal(ExpeditionStatus.Delivered, expedition.Status);
        Assert.NotNull(expedition.DeliveredAtUtc);
        Assert.Equal(DeliveryFailureReason.None, expedition.FailureReason);
        Assert.Equal(string.Empty, expedition.FailureDetails);
    }

    [Fact]
    public void MarkAsDeliveryFailed_ShouldRequireInTransitStatus()
    {
        var expedition = CreateExpedition();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            expedition.MarkAsDeliveryFailed(DeliveryFailureReason.AddressNotFound, "Street missing"));

        Assert.Equal("Only expeditions in transit can be marked as delivery failed.", exception.Message);
    }

    [Fact]
    public void MarkAsPickedUp_ShouldThrow_WhenStatusIsNotAwaitingPickup()
    {
        var expedition = CreateExpedition();
        expedition.MarkAsPickedUp();

        var exception = Assert.Throws<InvalidOperationException>(() => expedition.MarkAsPickedUp());

        Assert.Equal("Only expeditions awaiting pickup can be marked as picked up.", exception.Message);
    }

    [Fact]
    public void MarkAsInTransit_ShouldThrow_WhenStatusIsNotPickedUp()
    {
        var expedition = CreateExpedition();

        var exception = Assert.Throws<InvalidOperationException>(() => expedition.MarkAsInTransit());

        Assert.Equal("Only picked up expeditions can move to in transit.", exception.Message);
    }

    [Fact]
    public void MarkAsDelivered_ShouldThrow_WhenStatusIsNotInTransit()
    {
        var expedition = CreateExpedition();
        expedition.MarkAsPickedUp();

        var exception = Assert.Throws<InvalidOperationException>(() => expedition.MarkAsDelivered());

        Assert.Equal("Only expeditions in transit can be marked as delivered.", exception.Message);
    }

    [Fact]
    public void MarkAsDeliveryFailed_ShouldSetFailureReasonAndDetails()
    {
        var expedition = CreateExpedition();
        expedition.MarkAsPickedUp();
        expedition.MarkAsInTransit();

        expedition.MarkAsDeliveryFailed(DeliveryFailureReason.RecipientUnavailable, "Nobody home");

        Assert.Equal(ExpeditionStatus.DeliveryFailed, expedition.Status);
        Assert.Equal(DeliveryFailureReason.RecipientUnavailable, expedition.FailureReason);
        Assert.Equal("Nobody home", expedition.FailureDetails);
        Assert.NotNull(expedition.FailedAtUtc);
    }

    [Fact]
    public void MarkAsDeliveryFailed_ShouldThrow_WhenReasonIsNone()
    {
        var expedition = CreateExpedition();
        expedition.MarkAsPickedUp();
        expedition.MarkAsInTransit();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            expedition.MarkAsDeliveryFailed(DeliveryFailureReason.None, "details"));

        Assert.Equal("A failure reason must be provided for a failed delivery.", exception.Message);
    }

    private static ExpeditionOrder CreateExpedition()
    {
        return new ExpeditionOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            123,
            "1",
            "ACCESS-KEY",
            DateTime.UtcNow);
    }
}
