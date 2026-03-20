using Auth.Infrastructure.Security;

namespace Auth.Tests.Infrastructure;

public class BCryptPasswordHasherTests
{
    [Fact]
    public void Hash_ShouldReturnDifferentValueFromOriginalPassword()
    {
        var hasher = new BCryptPasswordHasher();

        var hash = hasher.Hash("secret123");

        Assert.NotEqual("secret123", hash);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.Hash("secret123");

        var result = hasher.Verify("secret123", hash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.Hash("secret123");

        var result = hasher.Verify("wrong-password", hash);

        Assert.False(result);
    }
}
