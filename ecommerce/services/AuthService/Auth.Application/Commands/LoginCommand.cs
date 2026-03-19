using System.ComponentModel.DataAnnotations;
using Auth.Application.DTOs;
using MediatR;

namespace Auth.Application.Commands;

public class LoginCommand : IRequest<AuthResponseDto>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
