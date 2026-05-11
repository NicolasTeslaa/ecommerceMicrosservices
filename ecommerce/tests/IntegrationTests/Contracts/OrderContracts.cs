namespace IntegrationTests.Contracts;

public sealed class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public Guid CustomerAddressId { get; set; }
    public decimal ShippingAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentToken { get; set; }
    public string? PaymentCardBrand { get; set; }
    public string? PaymentCardLast4 { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public sealed class CreateOrderItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public sealed class OrderProcessingAcceptedResponse
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; }
}

public sealed class OrderResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerAddressId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal ShippingAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public OrderRejectionReason? RejectionReason { get; set; }
    public string? RejectionDetail { get; set; }
    public IReadOnlyCollection<OrderItemResponse> Items { get; set; } = Array.Empty<OrderItemResponse>();
}

public sealed class OrderItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public enum PaymentMethod
{
    Credit = 1,
    Debit = 2,
    Pix = 3
}

public enum OrderStatus
{
    PendingPayment = 1,
    Pending = PendingPayment,
    Confirmed = 2,
    Cancelled = 3,
    PaymentRejected = 4
}

public enum OrderRejectionReason
{
    None = 0,
    ProductUnavailable = 1,
    InsufficientStock = 2,
    InvalidCustomerAddress = 3,
    ValidationFailed = 4,
    PaymentDeclined = 5
}
