namespace PharmaCore.Application.Customers.Dtos;

public sealed record UnpaidSaleDto(
    int SaleId,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    DateTime? CreatedAt);
