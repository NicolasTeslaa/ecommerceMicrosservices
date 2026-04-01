using Expedition.Domain.Enums;

namespace Expedition.Domain.Entities;

public class ExpeditionOrder
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public long InvoiceNumber { get; private set; }
    public string InvoiceSeries { get; private set; } = string.Empty;
    public string InvoiceAccessKey { get; private set; } = string.Empty;
    public ExpeditionStatus Status { get; private set; }
    public DeliveryFailureReason FailureReason { get; private set; }
    public string FailureDetails { get; private set; } = string.Empty;
    public DateTime InvoiceIssuedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? PickedUpAtUtc { get; private set; }
    public DateTime? InTransitAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? FailedAtUtc { get; private set; }

    private ExpeditionOrder()
    {
    }

    public ExpeditionOrder(
        Guid orderId,
        Guid invoiceId,
        Guid customerId,
        long invoiceNumber,
        string invoiceSeries,
        string invoiceAccessKey,
        DateTime invoiceIssuedAtUtc)
    {
        if (orderId == Guid.Empty)
            throw new InvalidOperationException("OrderId must be provided.");
        if (invoiceId == Guid.Empty)
            throw new InvalidOperationException("InvoiceId must be provided.");
        if (customerId == Guid.Empty)
            throw new InvalidOperationException("CustomerId must be provided.");
        if (invoiceNumber <= 0)
            throw new InvalidOperationException("InvoiceNumber must be greater than zero.");
        if (string.IsNullOrWhiteSpace(invoiceSeries))
            throw new InvalidOperationException("InvoiceSeries must be provided.");
        if (string.IsNullOrWhiteSpace(invoiceAccessKey))
            throw new InvalidOperationException("InvoiceAccessKey must be provided.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        InvoiceId = invoiceId;
        CustomerId = customerId;
        InvoiceNumber = invoiceNumber;
        InvoiceSeries = invoiceSeries.Trim();
        InvoiceAccessKey = invoiceAccessKey.Trim();
        Status = ExpeditionStatus.AwaitingCarrierPickup;
        FailureReason = DeliveryFailureReason.None;
        InvoiceIssuedAtUtc = invoiceIssuedAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void MarkAsPickedUp()
    {
        EnsureCurrentStatus(ExpeditionStatus.AwaitingCarrierPickup, "Only expeditions awaiting pickup can be marked as picked up.");

        Status = ExpeditionStatus.PickedUpByCarrier;
        PickedUpAtUtc = DateTime.UtcNow;
        ClearFailure();
        Touch();
    }

    public void MarkAsInTransit()
    {
        EnsureCurrentStatus(ExpeditionStatus.PickedUpByCarrier, "Only picked up expeditions can move to in transit.");

        Status = ExpeditionStatus.InTransit;
        InTransitAtUtc = DateTime.UtcNow;
        ClearFailure();
        Touch();
    }

    public void MarkAsDelivered()
    {
        EnsureCurrentStatus(ExpeditionStatus.InTransit, "Only expeditions in transit can be marked as delivered.");

        Status = ExpeditionStatus.Delivered;
        DeliveredAtUtc = DateTime.UtcNow;
        ClearFailure();
        Touch();
    }

    public void MarkAsDeliveryFailed(DeliveryFailureReason reason, string? details)
    {
        EnsureCurrentStatus(ExpeditionStatus.InTransit, "Only expeditions in transit can be marked as delivery failed.");

        if (reason == DeliveryFailureReason.None)
            throw new InvalidOperationException("A failure reason must be provided for a failed delivery.");

        Status = ExpeditionStatus.DeliveryFailed;
        FailureReason = reason;
        FailureDetails = details?.Trim() ?? string.Empty;
        FailedAtUtc = DateTime.UtcNow;
        Touch();
    }

    private void EnsureCurrentStatus(ExpeditionStatus expectedStatus, string message)
    {
        if (Status != expectedStatus)
            throw new InvalidOperationException(message);
    }

    private void ClearFailure()
    {
        FailureReason = DeliveryFailureReason.None;
        FailureDetails = string.Empty;
        FailedAtUtc = null;
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
