using Microsoft.EntityFrameworkCore;
using Payment.Domain.Exceptions;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Support;

namespace Payment.Tests.Infrastructure;

public class PaymentRepositoryTests
{
    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnPayment_WhenPaymentExists()
    {
        await using var context = CreateDbContext();
        var repository = new PaymentRepository(context);
        var payment = PaymentTestData.CreatePayment();
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var result = await repository.GetByOrderIdAsync(payment.OrderId);

        Assert.NotNull(result);
        Assert.Equal(payment.OrderId, result!.OrderId);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnNull_WhenPaymentDoesNotExist()
    {
        await using var context = CreateDbContext();
        var repository = new PaymentRepository(context);

        var result = await repository.GetByOrderIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByStripePaymentIntentIdAsync_ShouldReturnPayment_WhenIntentExists()
    {
        await using var context = CreateDbContext();
        var repository = new PaymentRepository(context);
        var payment = PaymentTestData.CreatePaymentWithIntent();
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var result = await repository.GetByStripePaymentIntentIdAsync(payment.StripePaymentIntentId!);

        Assert.NotNull(result);
        Assert.Equal(payment.StripePaymentIntentId, result!.StripePaymentIntentId);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistPayment()
    {
        await using var context = CreateDbContext();
        var repository = new PaymentRepository(context);
        var payment = PaymentTestData.CreatePayment();

        await repository.AddAsync(payment);

        Assert.Equal(1, await context.Payments.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistModifiedPayment()
    {
        await using var context = CreateDbContext();
        var repository = new PaymentRepository(context);
        var payment = PaymentTestData.CreatePayment();
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        payment.MarkCancelled("done");
        await repository.UpdateAsync(payment);

        var stored = await context.Payments.SingleAsync();
        Assert.Equal(PaymentStatus.Cancelled, stored.Status);
    }

    private static PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PaymentDbContext(options);
    }
}
