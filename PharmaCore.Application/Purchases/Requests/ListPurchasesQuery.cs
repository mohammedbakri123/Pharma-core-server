using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Purchases.Requests;

public sealed record ListPurchasesQuery(
    int Page,
    int Limit,
    int? SupplierId,
    PurchaseStatus? Status,
    DateTime? From,
    DateTime? To
);
