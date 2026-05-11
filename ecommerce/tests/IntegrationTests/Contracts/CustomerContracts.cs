namespace IntegrationTests.Contracts;

public sealed class CustomerResponse
{
    public Guid Id { get; set; }
    public Guid AuthUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
