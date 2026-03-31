namespace Order.Application.Interfaces;

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default);
    Task PublishOrderConfirmedAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default);
    Task PublishOrderRejectedAsync(
        Guid orderId,
        Guid customerId,
        Guid customerAddressId,
        DateTime requestedAtUtc,
        string reason,
        IReadOnlyCollection<Order.Application.DTOs.ProductAvailabilityIssueDto> issues,
        CancellationToken cancellationToken = default);
}
