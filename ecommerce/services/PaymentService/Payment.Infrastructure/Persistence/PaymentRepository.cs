using Microsoft.EntityFrameworkCore;
using Payment.Application.Interfaces;
using Payment.Domain.Exceptions;

namespace Payment.Infrastructure.Persistence;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Payment.Domain.Entities.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments.FirstOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException($"Failed to retrieve payment for order '{orderId}'.", exception);
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
            throw new PersistenceException($"Failed to retrieve payment for PaymentIntent '{paymentIntentId}'.", exception);
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
            throw new PersistenceException($"Failed to persist payment for order '{payment.OrderId}'.", exception);
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
            throw new PersistenceException($"Failed to update payment for order '{payment.OrderId}'.", exception);
        }
    }
}
