using Auth.Domain.Exceptions;
using Auth.Infrastructure.Persistence;
using Auth.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Auth.Tests.Infrastructure;

public class AuthUserRepositoryTests
{
    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        await using var dbContext = CreateDbContext();
        var repository = new AuthUserRepository(dbContext);
        var user = AuthTestData.CreateUser();
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var result = await repository.GetByEmailAsync(user.Email);

        Assert.NotNull(result);
        Assert.Equal(user.Email, result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var repository = new AuthUserRepository(dbContext);

        var result = await repository.GetByEmailAsync("missing@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldTrackUser_WhenUserIsValid()
    {
        await using var dbContext = CreateDbContext();
        var repository = new AuthUserRepository(dbContext);
        var user = AuthTestData.CreateUser();

        await repository.AddAsync(user);
        await dbContext.SaveChangesAsync();

        Assert.Equal(1, await dbContext.Users.CountAsync());
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldThrowPersistenceException_WhenContextIsDisposed()
    {
        var dbContext = CreateDbContext();
        var repository = new AuthUserRepository(dbContext);
        await dbContext.DisposeAsync();

        var act = () => repository.GetByEmailAsync("jane@example.com");

        await Assert.ThrowsAsync<PersistenceException>(act);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowPersistenceException_WhenContextIsDisposed()
    {
        var dbContext = CreateDbContext();
        var repository = new AuthUserRepository(dbContext);
        var user = AuthTestData.CreateUser();
        await dbContext.DisposeAsync();

        var act = () => repository.AddAsync(user);

        await Assert.ThrowsAsync<PersistenceException>(act);
    }

    private static AuthDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options);
    }
}
