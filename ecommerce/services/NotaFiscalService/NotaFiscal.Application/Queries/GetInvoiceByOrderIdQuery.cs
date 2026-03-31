using MediatR;
using NotaFiscal.Application.DTOs;

namespace NotaFiscal.Application.Queries;

public record GetInvoiceByOrderIdQuery(Guid OrderId) : IRequest<InvoiceDto?>;
