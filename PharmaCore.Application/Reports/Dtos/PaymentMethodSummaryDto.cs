namespace PharmaCore.Application.Reports.Dtos;

public sealed record PaymentMethodSummaryDto(
    PaymentSummaryDto Cash,
    PaymentSummaryDto Card);
