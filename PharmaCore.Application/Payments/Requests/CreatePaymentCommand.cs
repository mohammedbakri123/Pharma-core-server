using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Payments.Requests;

public sealed record CreatePaymentCommand(
    PaymentReferenceType ReferenceType,
    int ReferenceId,
    PaymentMethod Method,
    decimal Amount,
    string? Description,
    int? UserId);
