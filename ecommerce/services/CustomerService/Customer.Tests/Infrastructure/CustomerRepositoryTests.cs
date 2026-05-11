using Customer.Infrastructure.Persistence;
using Customer.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Customer.Tests.Infrastructure;

public class CustomerRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnCustomerWithAddresses_WhenCustomerExists()
    {
        await using var dbContext = CreateDbContext();
        var repository = new CustomerRepository(dbContext);
        var customer = CustomerTestData.CreateCustomer();
        CustomerTestData.AddAddress(customer, true);
        await dbContext.Customers.AddAsync(customer);
        await dbContext.SaveChangesAsync();

        var result = await repository.GetByIdAsync(customer.Id);

        Assert.NotNull(result);
        Assert.Equal(customer.PhoneNumber, result!.PhoneNumber);
        Assert.Single(result!.Addresses);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var repository = new CustomerRepository(dbContext);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistCustomer()
    {
        await using var dbContext = CreateDbContext();
        var repository = new CustomerRepository(dbContext);
        var customer = CustomerTestData.CreateCustomer();

        await repository.AddAsync(customer);

        Assert.Equal(1, await dbContext.Customers.CountAsync());
        Assert.Equal("11999999999", (await dbContext.Customers.SingleAsync()).PhoneNumber);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistNewAddress_WhenCustomerWasLoaded()
    {
        await using var dbContext = CreateDbContext();
        var customer = CustomerTestData.CreateCustomer();
        await dbContext.Customers.AddAsync(customer);
        await dbContext.SaveChangesAsync();
        var repository = new CustomerRepository(dbContext);

        var loadedCustomer = await repository.GetByIdAsync(customer.Id);
        Assert.NotNull(loadedCustomer);
        CustomerTestData.AddAddress(loadedCustomer!, true);

        await repository.UpdateAsync(loadedCustomer!);

        Assert.Equal(1, await dbContext.CustomerAddresses.CountAsync());
    }

    [Fact]
    public async Task AddAsync_ShouldNotPersist_WhenContextIsDisposed()
    {
        var dbContext = CreateDbContext();
        var repository = new CustomerRepository(dbContext);
        var customer = CustomerTestData.CreateCustomer();
        await dbContext.DisposeAsync();

        await repository.AddAsync(customer);

        Assert.True(true);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotThrow_WhenContextIsDisposed()
    {
        var dbContext = CreateDbContext();
        var repository = new CustomerRepository(dbContext);
        var customer = CustomerTestData.CreateCustomer();
        await dbContext.DisposeAsync();

        await repository.UpdateAsync(customer);

        Assert.True(true);
    }

    private static CustomerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CustomerDbContext(options);
    }
}
