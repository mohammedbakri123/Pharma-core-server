namespace PharmaCore.Application.Sales.Dtos;

public sealed record SaleBalanceDto(
    int SaleId,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount);