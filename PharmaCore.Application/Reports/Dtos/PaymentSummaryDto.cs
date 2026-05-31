namespace PharmaCore.Application.Reports.Dtos;

public sealed record PaymentSummaryDto(
    decimal In,
    decimal Out,
    decimal Net);
