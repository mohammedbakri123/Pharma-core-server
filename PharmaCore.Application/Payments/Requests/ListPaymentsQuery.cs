using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Payments.Requests;

public sealed record ListPaymentsQuery(
    int Page,
    int Limit,
    PaymentType? Type,
    PaymentMethod? Method,
    PaymentReferenceType? ReferenceType,
    DateTime? From,
    DateTime? To);
