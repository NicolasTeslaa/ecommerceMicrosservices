namespace Auth.Application.DTOs;

public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
