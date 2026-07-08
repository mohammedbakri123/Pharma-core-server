using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Payments.Dtos;

public sealed record PaymentsOverviewDto(
    PaymentOverviewSummaryDto Summary,
    IReadOnlyList<PaymentOverviewItemDto> Payments,
    PaymentOverviewPaginationDto Pagination);

public sealed record PaymentOverviewPaginationDto(int Total, int Page, int Limit);

public sealed record PaymentOverviewSummaryDto(
    decimal TotalIn,
    decimal TotalOut,
    decimal Net,
    PaymentOverviewMethodSummaryDto Cash,
    PaymentOverviewMethodSummaryDto Card,
    PaymentOverviewReferenceSummaryDto ByReferenceType);

public sealed record PaymentOverviewMethodSummaryDto(decimal In, decimal Out, decimal Net);

public sealed record PaymentOverviewReferenceSummaryDto(
    decimal Sale,
    decimal Purchase,
    decimal Expense,
    decimal SalesReturn,
    decimal PurchaseReturn);

public sealed record PaymentOverviewItemDto(
    int PaymentId,
    PaymentType Type,
    PaymentReferenceType ReferenceType,
    int ReferenceId,
    int? ParentReferenceId,
    PaymentMethod? Method,
    int? UserId,
    string? UserName,
    decimal Amount,
    string? Description,
    DateTime? CreatedAt,
    string ReferenceLabel,
    string? PartyName,
    decimal? ReferenceTotal);
