using System.Security.Claims;
using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Domain.Enums;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterUserCommand command)
    {
        var response = await _mediator.Send(command);
        return Created(string.Empty, ApiResponse<AuthResponseDto>.Ok(response, "User registered successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "User authenticated successfully."));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResponse<object>> Me()
    {
        var payload = new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"),
            CustomerId = User.FindFirstValue("customerId"),
            Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
            FullName = User.FindFirstValue("fullName")
        };

        return Ok(ApiResponse<object>.Ok(payload, "Authenticated user retrieved successfully."));
    }
}
