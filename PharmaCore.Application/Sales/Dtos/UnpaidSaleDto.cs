namespace PharmaCore.Application.Sales.Dtos;

public sealed record UnpaidSaleDto(
    int SaleId,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    DateTime CreatedAt);