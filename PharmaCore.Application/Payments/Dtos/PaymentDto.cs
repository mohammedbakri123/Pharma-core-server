using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Payments.Dtos;

public sealed record PaymentDto(
    int PaymentId,
    PaymentType Type,
    PaymentReferenceType ReferenceType,
    int ReferenceId,
    PaymentMethod? Method,
    int? UserId,
    string? UserName,
    decimal Amount,
    string? Description,
    DateTime? CreatedAt);
