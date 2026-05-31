using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Sales.Requests;

public sealed record ListSalesQuery(
    int Page,
    int Limit,
    int? CustomerId,
    int? UserId,
    SaleStatus? Status,
    DateTime? From,
    DateTime? To);