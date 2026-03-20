using System.Net.Mail;
using Order.Domain.Enums;
using Order.Domain.Exceptions;

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

    public Order(
        Guid id,
        Guid customerId,
        Guid customerAddressId,
        string customerEmail,
        string shippingAddress,
        decimal shippingAmount,
        PaymentMethod paymentMethod,
        string? paymentToken,
        string? paymentCardBrand,
        string? paymentCardLast4,
        IEnumerable<OrderItem> items,
        DateTime createdAtUtc)
    {
        var materializedItems = items?.ToList() ?? [];
        Validate(
            customerId,
            customerAddressId,
            customerEmail,
            shippingAddress,
            shippingAmount,
            paymentMethod,
            paymentToken,
            paymentCardBrand,
            paymentCardLast4,
            materializedItems);

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        CustomerId = customerId;
        CustomerAddressId = customerAddressId;
        CustomerEmail = customerEmail.Trim();
        ShippingAddress = shippingAddress.Trim();
        ShippingAmount = shippingAmount;
        PaymentMethod = paymentMethod;
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

    public Order(
        Guid customerId,
        Guid customerAddressId,
        string customerEmail,
        string shippingAddress,
        decimal shippingAmount,
        PaymentMethod paymentMethod,
        string? paymentToken,
        string? paymentCardBrand,
        string? paymentCardLast4,
        IEnumerable<OrderItem> items)
    {
        var materializedItems = items?.ToList() ?? [];
        Validate(
            customerId,
            customerAddressId,
            customerEmail,
            shippingAddress,
            shippingAmount,
            paymentMethod,
            paymentToken,
            paymentCardBrand,
            paymentCardLast4,
            materializedItems);

        Id = Guid.NewGuid();
        CustomerId = customerId;
        CustomerAddressId = customerAddressId;
        CustomerEmail = customerEmail.Trim();
        ShippingAddress = shippingAddress.Trim();
        ShippingAmount = shippingAmount;
        PaymentMethod = paymentMethod;
        PaymentToken = string.IsNullOrWhiteSpace(paymentToken) ? null : paymentToken.Trim();
        PaymentCardBrand = string.IsNullOrWhiteSpace(paymentCardBrand) ? null : paymentCardBrand.Trim();
        PaymentCardLast4 = string.IsNullOrWhiteSpace(paymentCardLast4) ? null : paymentCardLast4.Trim();
        Status = OrderStatus.PendingPayment;
        RejectionReason = null;
        RejectionDetail = null;
        CreatedAtUtc = DateTime.UtcNow;
        _items.AddRange(materializedItems);
        TotalAmount = _items.Sum(item => item.TotalPrice) + ShippingAmount;
    }

    public static Order CreateRejected(
        Guid id,
        Guid customerId,
        Guid customerAddressId,
        decimal shippingAmount,
        PaymentMethod paymentMethod,
        string? paymentToken,
        string? paymentCardBrand,
        string? paymentCardLast4,
        IEnumerable<OrderItem> items,
        DateTime createdAtUtc,
        OrderRejectionReason rejectionReason,
        string rejectionDetail,
        string? customerEmail = null,
        string? shippingAddress = null)
    {
        var materializedItems = items?.ToList() ?? [];

        if (customerId == Guid.Empty)
            throw new InvalidCustomerIdException();

        if (customerAddressId == Guid.Empty)
            throw new InvalidCustomerAddressIdException();

        if (!Enum.IsDefined(paymentMethod))
            throw new InvalidPaymentMethodException();

        if (materializedItems.Count == 0)
            throw new InvalidOrderItemException();

        var order = new Order
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            CustomerId = customerId,
            CustomerAddressId = customerAddressId,
            CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? "rejected@order.local" : customerEmail.Trim(),
            ShippingAddress = string.IsNullOrWhiteSpace(shippingAddress) ? "Order rejected before confirmation." : shippingAddress.Trim(),
            ShippingAmount = shippingAmount,
            PaymentMethod = paymentMethod,
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

    private static void Validate(
        Guid customerId,
        Guid customerAddressId,
        string customerEmail,
        string shippingAddress,
        decimal shippingAmount,
        PaymentMethod paymentMethod,
        string? paymentToken,
        string? paymentCardBrand,
        string? paymentCardLast4,
        IReadOnlyCollection<OrderItem> items)
    {
        if (customerId == Guid.Empty)
            throw new InvalidCustomerIdException();

        if (customerAddressId == Guid.Empty)
            throw new InvalidCustomerAddressIdException();

        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new InvalidCustomerEmailException();

        try
        {
            _ = new MailAddress(customerEmail);
        }
        catch (FormatException)
        {
            throw new InvalidCustomerEmailException();
        }

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new InvalidShippingAddressException();

        if (shippingAmount < 0)
            throw new InvalidShippingAddressException();

        if (!Enum.IsDefined(paymentMethod))
            throw new InvalidPaymentMethodException();

        var requiresCardToken = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit;

        if (requiresCardToken && string.IsNullOrWhiteSpace(paymentToken))
            throw new InvalidPaymentTokenException();

        if (requiresCardToken && (string.IsNullOrWhiteSpace(paymentCardBrand) || string.IsNullOrWhiteSpace(paymentCardLast4)))
            throw new InvalidPaymentCardDataException();

        if (requiresCardToken && (paymentCardLast4?.Trim().Length != 4 || paymentCardLast4.Any(character => !char.IsDigit(character))))
            throw new InvalidPaymentCardDataException();

        if (items.Count == 0)
            throw new InvalidOrderItemException();
    }
}
