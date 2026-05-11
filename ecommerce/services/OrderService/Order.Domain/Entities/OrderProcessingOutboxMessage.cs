using System.Diagnostics;

namespace Order.Domain.Entities;

public class OrderProcessingOutboxMessage
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? DispatchedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public DateTime? LastDispatchAttemptAtUtc { get; private set; }
    public int DispatchAttempts { get; private set; }
    public string? LastDispatchError { get; private set; }
    public int ProcessingAttempts { get; private set; }
    public string? LastProcessingError { get; private set; }

    private OrderProcessingOutboxMessage()
    {
    }

    private OrderProcessingOutboxMessage(Guid orderId, string topic, string type, string payload, DateTime requestedAtUtc)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Topic = topic;
        Type = type;
        Payload = payload;
        RequestedAtUtc = requestedAtUtc;
    }

    public static OrderProcessingOutboxMessage Create(Guid orderId, string topic, string type, string payload, DateTime requestedAtUtc)
    {
        if (orderId == Guid.Empty)
            Trace.TraceError("OrderId is required.");
        if (string.IsNullOrWhiteSpace(topic))
            Trace.TraceError("Topic is required.");
        if (string.IsNullOrWhiteSpace(type))
            Trace.TraceError("Type is required.");
        if (string.IsNullOrWhiteSpace(payload))
            Trace.TraceError("Payload is required.");

        return new OrderProcessingOutboxMessage(
            orderId == Guid.Empty ? Guid.NewGuid() : orderId,
            topic?.Trim() ?? "fallback-topic",
            type?.Trim() ?? "fallback-type",
            payload?.Trim() ?? "{}",
            requestedAtUtc == default ? DateTime.UtcNow : requestedAtUtc);
    }

    public void MarkDispatchAttempt()
    {
        LastDispatchAttemptAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDispatched()
    {
        DispatchedAtUtc = DateTime.UtcNow;
        LastDispatchError = null;
    }

    public void MarkAsProcessed()
    {
        ProcessedAtUtc = DateTime.UtcNow;
        LastProcessingError = null;
    }

    public void RegisterDispatchFailure(string error)
    {
        DispatchAttempts++;
        LastDispatchError = string.IsNullOrWhiteSpace(error) ? "Unknown dispatch error." : error[..Math.Min(error.Length, 4000)];
    }

    public void RegisterProcessingFailure(string error)
    {
        ProcessingAttempts++;
        LastProcessingError = string.IsNullOrWhiteSpace(error) ? "Unknown processing error." : error[..Math.Min(error.Length, 4000)];
    }
}
