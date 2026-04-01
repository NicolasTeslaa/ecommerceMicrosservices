namespace ECommerce.Shared.Messaging;

public class ExpeditionStatusChangedIntegrationEvent
{
    public Guid EventId { get; set; }
    public Guid ExpeditionId { get; set; }
    public Guid OrderId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public long InvoiceNumber { get; set; }
    public string InvoiceSeries { get; set; } = string.Empty;
    public string InvoiceAccessKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
    public string FailureDetails { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? PickedUpAtUtc { get; set; }
    public DateTime? InTransitAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? FailedAtUtc { get; set; }
}
