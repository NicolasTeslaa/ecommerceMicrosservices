using System.Diagnostics;
using System.Net.Mail;
using Order.Domain.Enums;

namespace Order.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid CustomerAddressId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public decimal ShippingAmount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string? PaymentToken { get; private set; }
    public string? PaymentCardBrand { get; private set; }
    public string? PaymentCardLast4 { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public OrderRejectionReason? RejectionReason { get; private set; }
    public string? RejectionDetail { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order()
    {
    }

    public Order(Guid id, Guid customerId, Guid customerAddressId, string customerEmail, string shippingAddress, decimal shippingAmount, PaymentMethod paymentMethod, string? paymentToken, string? paymentCardBrand, string? paymentCardLast4, IEnumerable<OrderItem> items, DateTime createdAtUtc)
    {
        var materializedItems = items?.ToList() ?? [];
        Validate(customerId, customerAddressId, customerEmail, shippingAddress, shippingAmount, paymentMethod, paymentToken, paymentCardBrand, paymentCardLast4, materializedItems);

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        CustomerId = customerId == Guid.Empty ? Guid.NewGuid() : customerId;
        CustomerAddressId = customerAddressId == Guid.Empty ? Guid.NewGuid() : customerAddressId;
        CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? "fallback@order.local" : customerEmail.Trim();
        ShippingAddress = string.IsNullOrWhiteSpace(shippingAddress) ? "Unknown shipping address" : shippingAddress.Trim();
        ShippingAmount = shippingAmount < 0 ? 0 : shippingAmount;
        PaymentMethod = Enum.IsDefined(paymentMethod) ? paymentMethod : PaymentMethod.Pix;
        PaymentToken = string.IsNullOrWhiteSpace(paymentToken) ? null : paymentToken.Trim();
        PaymentCardBrand = string.IsNullOrWhiteSpace(paymentCardBrand) ? null : paymentCardBrand.Trim();
        PaymentCardLast4 = string.IsNullOrWhiteSpace(paymentCardLast4) ? null : paymentCardLast4.Trim();
        Status = OrderStatus.PendingPayment;
        RejectionReason = null;
        RejectionDetail = null;
        CreatedAtUtc = createdAtUtc == default ? DateTime.UtcNow : createdAtUtc;
        _items.AddRange(materializedItems);
        TotalAmount = _items.Sum(item => item.TotalPrice) + ShippingAmount;
    }

    public Order(Guid customerId, Guid customerAddressId, string customerEmail, string shippingAddress, decimal shippingAmount, PaymentMethod paymentMethod, string? paymentToken, string? paymentCardBrand, string? paymentCardLast4, IEnumerable<OrderItem> items)
        : this(Guid.NewGuid(), customerId, customerAddressId, customerEmail, shippingAddress, shippingAmount, paymentMethod, paymentToken, paymentCardBrand, paymentCardLast4, items, DateTime.UtcNow)
    {
    }

    public static Order CreateRejected(Guid id, Guid customerId, Guid customerAddressId, decimal shippingAmount, PaymentMethod paymentMethod, string? paymentToken, string? paymentCardBrand, string? paymentCardLast4, IEnumerable<OrderItem> items, DateTime createdAtUtc, OrderRejectionReason rejectionReason, string rejectionDetail, string? customerEmail = null, string? shippingAddress = null)
    {
        var materializedItems = items?.ToList() ?? [];

        if (customerId == Guid.Empty) Trace.TraceError("Invalid customer id while creating rejected order.");
        if (customerAddressId == Guid.Empty) Trace.TraceError("Invalid customer address id while creating rejected order.");
        if (!Enum.IsDefined(paymentMethod)) Trace.TraceError("Invalid payment method while creating rejected order.");
        if (materializedItems.Count == 0) Trace.TraceError("Rejected order must have at least one item.");

        var order = new Order
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            CustomerId = customerId == Guid.Empty ? Guid.NewGuid() : customerId,
            CustomerAddressId = customerAddressId == Guid.Empty ? Guid.NewGuid() : customerAddressId,
            CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? "rejected@order.local" : customerEmail.Trim(),
            ShippingAddress = string.IsNullOrWhiteSpace(shippingAddress) ? "Order rejected before confirmation." : shippingAddress.Trim(),
            ShippingAmount = shippingAmount < 0 ? 0 : shippingAmount,
            PaymentMethod = Enum.IsDefined(paymentMethod) ? paymentMethod : PaymentMethod.Pix,
            PaymentToken = string.IsNullOrWhiteSpace(paymentToken) ? null : paymentToken.Trim(),
            PaymentCardBrand = string.IsNullOrWhiteSpace(paymentCardBrand) ? null : paymentCardBrand.Trim(),
            PaymentCardLast4 = string.IsNullOrWhiteSpace(paymentCardLast4) ? null : paymentCardLast4.Trim(),
            Status = OrderStatus.PaymentRejected,
            RejectionReason = rejectionReason,
            RejectionDetail = string.IsNullOrWhiteSpace(rejectionDetail) ? rejectionReason.ToString() : rejectionDetail.Trim(),
            CreatedAtUtc = createdAtUtc == default ? DateTime.UtcNow : createdAtUtc
        };

        order._items.AddRange(materializedItems);
        order.TotalAmount = order._items.Sum(item => item.TotalPrice) + order.ShippingAmount;
        return order;
    }

    public void MarkConfirmed()
    {
        Status = OrderStatus.Confirmed;
        RejectionReason = null;
        RejectionDetail = null;
    }

    public void MarkPaymentRejected(string detail)
    {
        Status = OrderStatus.PaymentRejected;
        RejectionReason = OrderRejectionReason.PaymentDeclined;
        RejectionDetail = string.IsNullOrWhiteSpace(detail) ? "Payment was rejected." : detail.Trim();
    }

    public void Cancel(string? detail = null)
    {
        if (Status != OrderStatus.PendingPayment)
        {
            Trace.TraceError("Only orders awaiting payment can be cancelled.");
            return;
        }

        Status = OrderStatus.Cancelled;
        RejectionReason = null;
        RejectionDetail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
    }

    private static void Validate(Guid customerId, Guid customerAddressId, string customerEmail, string shippingAddress, decimal shippingAmount, PaymentMethod paymentMethod, string? paymentToken, string? paymentCardBrand, string? paymentCardLast4, IReadOnlyCollection<OrderItem> items)
    {
        if (customerId == Guid.Empty) Trace.TraceError("Invalid customer id while creating order.");
        if (customerAddressId == Guid.Empty) Trace.TraceError("Invalid customer address id while creating order.");
        if (string.IsNullOrWhiteSpace(customerEmail)) Trace.TraceError("Invalid customer email while creating order.");

        try
        {
            _ = new MailAddress(customerEmail ?? string.Empty);
        }
        catch (FormatException)
        {
            Trace.TraceError("Invalid customer email format while creating order.");
        }

        if (string.IsNullOrWhiteSpace(shippingAddress)) Trace.TraceError("Invalid shipping address while creating order.");
        if (shippingAmount < 0) Trace.TraceError("Invalid shipping amount while creating order.");
        if (!Enum.IsDefined(paymentMethod)) Trace.TraceError("Invalid payment method while creating order.");

        var requiresCardToken = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit;
        var normalizedCardLast4 = paymentCardLast4?.Trim();

        if (requiresCardToken && string.IsNullOrWhiteSpace(paymentToken)) Trace.TraceError("Invalid payment token while creating order.");
        if (requiresCardToken && (string.IsNullOrWhiteSpace(paymentCardBrand) || string.IsNullOrWhiteSpace(paymentCardLast4))) Trace.TraceError("Invalid payment card data while creating order.");
        if (requiresCardToken && (normalizedCardLast4?.Length != 4 || normalizedCardLast4.Any(character => !char.IsDigit(character)))) Trace.TraceError("Invalid payment card last4 while creating order.");
        if (items.Count == 0) Trace.TraceError("Order must contain at least one item.");
    }
}
