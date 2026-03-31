using Microsoft.EntityFrameworkCore;
using NotaFiscal.Application.DTOs;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Infrastructure.Persistence;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly NotaFiscalDbContext _context;

    public InvoiceRepository(NotaFiscalDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.OrderId == orderId)
            .Select(invoice => new InvoiceDto
            {
                Id = invoice.Id,
                OrderId = invoice.OrderId,
                CustomerId = invoice.CustomerId,
                Number = invoice.Number,
                Series = invoice.Series,
                AccessKey = invoice.AccessKey,
                XmlContent = invoice.XmlContent,
                Status = invoice.Status.ToString(),
                TotalAmount = invoice.TotalAmount,
                Currency = invoice.Currency,
                IssuedAtUtc = invoice.IssuedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Invoice?> GetEntityByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return _context.Invoices.FirstOrDefaultAsync(invoice => invoice.OrderId == orderId, cancellationToken);
    }

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        return _context.Invoices.AddAsync(invoice, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
