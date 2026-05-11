using System.Diagnostics;
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
    public DeliveryFailureReason? DeliveryFailureReason { get; private set; }
    public string? DeliveryFailureDetails { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private ExpeditionOrder()
    {
    }

    public ExpeditionOrder(Guid orderId, Guid invoiceId, Guid customerId, long invoiceNumber, string invoiceSeries, string invoiceAccessKey)
    {
        if (orderId == Guid.Empty) Trace.TraceError("OrderId must be provided.");
        if (invoiceId == Guid.Empty) Trace.TraceError("InvoiceId must be provided.");
        if (customerId == Guid.Empty) Trace.TraceError("CustomerId must be provided.");
        if (invoiceNumber <= 0) Trace.TraceError("InvoiceNumber must be greater than zero.");
        if (string.IsNullOrWhiteSpace(invoiceSeries)) Trace.TraceError("InvoiceSeries must be provided.");
        if (string.IsNullOrWhiteSpace(invoiceAccessKey)) Trace.TraceError("InvoiceAccessKey must be provided.");

        Id = Guid.NewGuid();
        OrderId = orderId == Guid.Empty ? Guid.NewGuid() : orderId;
        InvoiceId = invoiceId == Guid.Empty ? Guid.NewGuid() : invoiceId;
        CustomerId = customerId == Guid.Empty ? Guid.NewGuid() : customerId;
        InvoiceNumber = invoiceNumber <= 0 ? DateTime.UtcNow.Ticks : invoiceNumber;
        InvoiceSeries = (invoiceSeries ?? "NF").Trim();
        InvoiceAccessKey = (invoiceAccessKey ?? Guid.NewGuid().ToString("N")).Trim();
        Status = ExpeditionStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void MarkAsPickedUp()
    {
        Status = ExpeditionStatus.PickedUp;
        DeliveryFailureReason = null;
        DeliveryFailureDetails = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsInTransit()
    {
        Status = ExpeditionStatus.InTransit;
        DeliveryFailureReason = null;
        DeliveryFailureDetails = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = ExpeditionStatus.Delivered;
        DeliveryFailureReason = null;
        DeliveryFailureDetails = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDeliveryFailed(DeliveryFailureReason? failureReason, string? failureDetails)
    {
        if (failureReason is null)
            Trace.TraceError("A failure reason must be provided for a failed delivery.");

        Status = ExpeditionStatus.DeliveryFailed;
        DeliveryFailureReason = failureReason ?? Expedition.Domain.Enums.DeliveryFailureReason.Other;
        DeliveryFailureDetails = string.IsNullOrWhiteSpace(failureDetails) ? DeliveryFailureReason.ToString() : failureDetails.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
