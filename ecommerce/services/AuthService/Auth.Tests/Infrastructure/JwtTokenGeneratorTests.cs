using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Infrastructure.Configuration;
using Auth.Infrastructure.Security;
using Auth.Tests.Support;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Tests.Infrastructure;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnNonEmptyAccessToken()
    {
        var generator = CreateGenerator();
        var user = AuthTestData.CreateUser();

        var result = generator.Generate(user);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public void Generate_ShouldReturnExpirationInFuture()
    {
        var generator = CreateGenerator();
        var user = AuthTestData.CreateUser();

        var result = generator.Generate(user);

        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public void Generate_ShouldEmbedExpectedClaims()
    {
        var generator = CreateGenerator();
        var user = AuthTestData.CreateUser();
        var handler = new JwtSecurityTokenHandler();

        var result = generator.Generate(user);
        var token = handler.ReadJwtToken(result.AccessToken);

        Assert.Equal(user.Id.ToString(), token.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, token.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.CustomerId.ToString(), token.Claims.First(claim => claim.Type == "customerId").Value);
        Assert.Equal(user.FullName, token.Claims.First(claim => claim.Type == "fullName").Value);
    }

    [Fact]
    public void Generate_ShouldUseConfiguredIssuerAndAudience()
    {
        var generator = CreateGenerator();
        var user = AuthTestData.CreateUser();
        var handler = new JwtSecurityTokenHandler();

        var result = generator.Generate(user);
        var token = handler.ReadJwtToken(result.AccessToken);

        Assert.Equal("test-issuer", token.Issuer);
        Assert.Contains("test-audience", token.Audiences);
    }

    [Fact]
    public void Generate_ShouldCreateTokenSignedWithConfiguredSecret()
    {
        var options = new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = "12345678901234567890123456789012",
            AccessTokenExpirationMinutes = 60
        };
        var generator = new JwtTokenGenerator(Options.Create(options));
        var user = AuthTestData.CreateUser();
        var handler = new JwtSecurityTokenHandler();

        var result = generator.Generate(user);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey))
        };

        var principal = handler.ValidateToken(result.AccessToken, validationParameters, out _);

        Assert.NotNull(principal.FindFirst(ClaimTypes.Email) ?? principal.FindFirst(JwtRegisteredClaimNames.Email));
    }

    private static JwtTokenGenerator CreateGenerator()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = "12345678901234567890123456789012",
            AccessTokenExpirationMinutes = 60
        });

        return new JwtTokenGenerator(options);
    }
}
