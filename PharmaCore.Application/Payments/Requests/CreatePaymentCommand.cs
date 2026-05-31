using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Payments.Requests;

public sealed record CreatePaymentCommand(
    PaymentType Type,
    PaymentReferenceType ReferenceType,
    int ReferenceId,
    PaymentMethod Method,
    decimal Amount,
    string? Description,
    int? UserId);
