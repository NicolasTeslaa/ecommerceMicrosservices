using Microsoft.EntityFrameworkCore;
using NotaFiscal.Infrastructure.Persistence;
using NotaFiscal.Tests.Support;

namespace NotaFiscal.Tests.Infrastructure;

public class InvoiceRepositoryTests
{
    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnDto_WhenInvoiceExists()
    {
        await using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var invoice = NotaFiscalTestData.CreateInvoice();
        await context.Invoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        var result = await repository.GetByOrderIdAsync(invoice.OrderId);

        Assert.NotNull(result);
        Assert.Equal(invoice.OrderId, result!.OrderId);
        Assert.Equal(invoice.AccessKey, result.AccessKey);
    }

    [Fact]
    public async Task GetEntityByOrderIdAsync_ShouldReturnEntity_WhenInvoiceExists()
    {
        await using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var invoice = NotaFiscalTestData.CreateInvoice();
        await context.Invoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        var result = await repository.GetEntityByOrderIdAsync(invoice.OrderId);

        Assert.NotNull(result);
        Assert.Equal(invoice.Id, result!.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistInvoice()
    {
        await using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var invoice = NotaFiscalTestData.CreateInvoice();

        await repository.AddAsync(invoice);
        await repository.SaveChangesAsync();

        Assert.Equal(1, await context.Invoices.CountAsync());
    }

    private static NotaFiscalDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NotaFiscalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NotaFiscalDbContext(options);
    }
}
