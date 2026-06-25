using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Contracts.Payments;

public sealed record CreatePaymentRequest(
    PaymentReferenceType ReferenceType,
    int ReferenceId,
    PaymentMethod Method,
    decimal Amount,
    string? Description);
