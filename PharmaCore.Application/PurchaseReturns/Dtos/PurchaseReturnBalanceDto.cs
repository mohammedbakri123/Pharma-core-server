namespace PharmaCore.Application.PurchaseReturns.Dtos;

public sealed record PurchaseReturnBalanceDto(
    int PurchaseReturnId,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount);
