using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Application.Interfaces;

namespace Payment.Infrastructure.Persistence;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(PaymentDbContext context, ILogger<PaymentRepository>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<PaymentRepository>.Instance;
    }

    public async Task<Payment.Domain.Entities.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments.FirstOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve payment for order '{OrderId}'.", orderId);
            return null;
        }
    }

    public async Task<Payment.Domain.Entities.Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments.FirstOrDefaultAsync(
                payment => payment.StripePaymentIntentId == paymentIntentId,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve payment for PaymentIntent '{PaymentIntentId}'.", paymentIntentId);
            return null;
        }
    }

    public async Task AddAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Payments.AddAsync(payment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to persist payment for order '{OrderId}'.", payment.OrderId);
        }
    }

    public async Task UpdateAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to update payment for order '{OrderId}'.", payment.OrderId);
        }
    }
}
