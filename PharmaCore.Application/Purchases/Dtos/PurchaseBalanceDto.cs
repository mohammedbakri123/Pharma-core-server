namespace PharmaCore.Application.Purchases.Dtos;

public sealed record PurchaseBalanceDto(
    int PurchaseId,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount);
