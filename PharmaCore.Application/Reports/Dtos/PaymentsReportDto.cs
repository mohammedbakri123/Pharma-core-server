namespace PharmaCore.Application.Reports.Dtos;

public sealed record PaymentsReportDto(
    DateTime? From,
    DateTime? To,
    decimal TotalIn,
    decimal TotalOut,
    PaymentMethodSummaryDto ByMethod,
    PaymentsByReferenceDto ByReferenceType);
