using System.Security.Claims;
using Auth.API.Controllers;
using Auth.Application.Commands;
using Auth.Application.DTOs;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Auth.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnCreated_WhenMediatorReturnsResponse()
    {
        var command = new RegisterUserCommand { FullName = "Jane Doe", Email = "jane@example.com", Password = "secret123" };
        _mediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponseDto { UserId = Guid.NewGuid(), Email = command.Email });

        var result = await _controller.Register(command);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponseDto>>(created.Value);
        Assert.True(response.Success);
        Assert.Equal(command.Email, response.Data!.Email);
    }

    [Fact]
    public async Task Register_ShouldForwardCommandToMediator()
    {
        var command = new RegisterUserCommand { FullName = "Jane Doe", Email = "jane@example.com", Password = "secret123" };
        _mediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponseDto());

        await _controller.Register(command);

        _mediatorMock.Verify(mediator => mediator.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenMediatorReturnsResponse()
    {
        var command = new LoginCommand { Email = "jane@example.com", Password = "secret123" };
        _mediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponseDto { UserId = Guid.NewGuid(), Email = command.Email });

        var result = await _controller.Login(command);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponseDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(command.Email, response.Data!.Email);
    }

    [Fact]
    public async Task Login_ShouldForwardCommandToMediator()
    {
        var command = new LoginCommand { Email = "jane@example.com", Password = "secret123" };
        _mediatorMock.Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponseDto());

        await _controller.Login(command);

        _mediatorMock.Verify(mediator => mediator.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Me_ShouldReturnClaimsPayload_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim("customerId", customerId),
                    new Claim(ClaimTypes.Email, "jane@example.com"),
                    new Claim("fullName", "Jane Doe")
                ], "Test"))
            }
        };

        var result = _controller.Me();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public void Me_ShouldFallbackToSubAndEmailClaims_WhenStandardClaimsAreMissing()
    {
        var userId = Guid.NewGuid().ToString();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("sub", userId),
                    new Claim("customerId", Guid.NewGuid().ToString()),
                    new Claim("email", "jane@example.com"),
                    new Claim("fullName", "Jane Doe")
                ], "Test"))
            }
        };

        var result = _controller.Me();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.Success);
    }
}
