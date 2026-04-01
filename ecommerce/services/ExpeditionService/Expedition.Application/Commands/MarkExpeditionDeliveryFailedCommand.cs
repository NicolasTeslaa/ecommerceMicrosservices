using Expedition.Application.DTOs;
using MediatR;

namespace Expedition.Application.Commands;

public record MarkExpeditionDeliveryFailedCommand(
    Guid OrderId,
    string FailureReason,
    string? FailureDetails) : IRequest<ExpeditionDto>;
