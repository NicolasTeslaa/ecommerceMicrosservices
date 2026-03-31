using NotaFiscal.Application.DTOs;
using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetEntityByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
