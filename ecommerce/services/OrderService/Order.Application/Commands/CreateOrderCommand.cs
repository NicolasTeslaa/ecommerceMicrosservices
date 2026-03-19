using System.ComponentModel.DataAnnotations;
using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Commands;

public class CreateOrderCommand : IRequest<OrderProcessingAcceptedDto>
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid CustomerAddressId { get; set; }

    [Required]
    public decimal ShippingAmount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public decimal UnitPrice { get; set; }

    [Required]
    public int Quantity { get; set; }
}
