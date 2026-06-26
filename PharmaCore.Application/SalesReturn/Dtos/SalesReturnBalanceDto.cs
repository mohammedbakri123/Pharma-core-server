namespace PharmaCore.Application.SalesReturn.Dtos;

public sealed record SalesReturnBalanceDto(
    int SalesReturnId,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount);
