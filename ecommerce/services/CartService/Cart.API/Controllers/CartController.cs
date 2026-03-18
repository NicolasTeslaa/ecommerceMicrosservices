using Cart.Application.Commands;
using Cart.Application.DTOs;
using Cart.Application.Queries;
using Cart.Domain.Enums;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/cart/{ownerType}/{ownerId}")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart(CartOwnerType ownerType, string ownerId)
    {
        var cart = await _mediator.Send(new GetCartQuery(ownerId, ownerType));

        return Ok(ApiResponse<CartDto>.Ok(cart, "Cart retrieved successfully."));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddItem(
        CartOwnerType ownerType,
        string ownerId,
        [FromBody] AddCartItemCommand command)
    {
        command.OwnerId = ownerId;
        command.OwnerType = ownerType;

        var cart = await _mediator.Send(command);

        return Ok(ApiResponse<CartDto>.Ok(cart, "Cart item added successfully."));
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateItemQuantity(
        CartOwnerType ownerType,
        string ownerId,
        Guid productId,
        [FromBody] UpdateCartItemQuantityCommand command)
    {
        command.OwnerId = ownerId;
        command.OwnerType = ownerType;
        command.ProductId = productId;

        var cart = await _mediator.Send(command);

        return Ok(ApiResponse<CartDto>.Ok(cart, "Cart item quantity updated successfully."));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> RemoveItem(CartOwnerType ownerType, string ownerId, Guid productId)
    {
        var cart = await _mediator.Send(new RemoveCartItemCommand
        {
            OwnerId = ownerId,
            OwnerType = ownerType,
            ProductId = productId
        });

        return Ok(ApiResponse<CartDto>.Ok(cart, "Cart item removed successfully."));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<CartDto>>> Clear(CartOwnerType ownerType, string ownerId)
    {
        var cart = await _mediator.Send(new ClearCartCommand
        {
            OwnerId = ownerId,
            OwnerType = ownerType
        });

        return Ok(ApiResponse<CartDto>.Ok(cart, "Cart cleared successfully."));
    }
}
