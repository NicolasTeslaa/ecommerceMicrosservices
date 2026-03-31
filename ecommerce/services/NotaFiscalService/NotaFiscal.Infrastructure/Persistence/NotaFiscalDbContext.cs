using Microsoft.EntityFrameworkCore;
using NotaFiscal.Domain.Entities;
using NotaFiscal.Infrastructure.Configuration;

namespace NotaFiscal.Infrastructure.Persistence;

public class NotaFiscalDbContext : DbContext
{
    public NotaFiscalDbContext(DbContextOptions<NotaFiscalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<ProcessedKafkaMessage> ProcessedKafkaMessages => Set<ProcessedKafkaMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedKafkaMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
