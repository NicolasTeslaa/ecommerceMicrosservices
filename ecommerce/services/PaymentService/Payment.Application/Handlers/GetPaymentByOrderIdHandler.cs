using MediatR;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Application.Queries;

namespace Payment.Application.Handlers;

public class GetPaymentByOrderIdHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentByOrderIdHandler(IPaymentRepository repository) => _repository = repository;

    public async Task<PaymentDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            return new PaymentDto { OrderId = Guid.Empty };

        var payment = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null)
            return new PaymentDto { OrderId = request.OrderId };

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentMethod = payment.PaymentMethod,
            StripePaymentIntentId = payment.StripePaymentIntentId,
            StripeClientSecret = payment.StripeClientSecret,
            Status = payment.Status,
            FailureReason = payment.FailureReason,
            FailureDetail = payment.FailureDetail,
            AttemptCount = payment.AttemptCount,
            MaxAttemptsReached = payment.HasReachedMaxAttempts,
            CreatedAtUtc = payment.CreatedAtUtc,
            UpdatedAtUtc = payment.UpdatedAtUtc
        };
    }
}
