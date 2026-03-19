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
    public string PaymentMethod { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
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
        string paymentMethod,
        IEnumerable<OrderItem> items,
        DateTime createdAtUtc)
    {
        var materializedItems = items?.ToList() ?? [];
        Validate(customerId, customerAddressId, customerEmail, shippingAddress, shippingAmount, paymentMethod, materializedItems);

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        CustomerId = customerId;
        CustomerAddressId = customerAddressId;
        CustomerEmail = customerEmail.Trim();
        ShippingAddress = shippingAddress.Trim();
        ShippingAmount = shippingAmount;
        PaymentMethod = paymentMethod.Trim();
        Status = OrderStatus.Pending;
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
        string paymentMethod,
        IEnumerable<OrderItem> items)
    {
        var materializedItems = items?.ToList() ?? [];
        Validate(customerId, customerAddressId, customerEmail, shippingAddress, shippingAmount, paymentMethod, materializedItems);

        Id = Guid.NewGuid();
        CustomerId = customerId;
        CustomerAddressId = customerAddressId;
        CustomerEmail = customerEmail.Trim();
        ShippingAddress = shippingAddress.Trim();
        ShippingAmount = shippingAmount;
        PaymentMethod = paymentMethod.Trim();
        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        _items.AddRange(materializedItems);
        TotalAmount = _items.Sum(item => item.TotalPrice) + ShippingAmount;
    }

    private static void Validate(
        Guid customerId,
        Guid customerAddressId,
        string customerEmail,
        string shippingAddress,
        decimal shippingAmount,
        string paymentMethod,
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

        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new InvalidPaymentMethodException();

        if (items.Count == 0)
            throw new InvalidOrderItemException();
    }
}
