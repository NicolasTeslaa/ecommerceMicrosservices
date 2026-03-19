using Order.Domain.Enums;

namespace Order.Domain.Exceptions;

public class OrderNotFoundException : OrderException
{
    public OrderNotFoundException(Guid orderId)
        : base(OrderErrorCode.OrderNotFound, $"Order '{orderId}' was not found.")
    {
    }
}
