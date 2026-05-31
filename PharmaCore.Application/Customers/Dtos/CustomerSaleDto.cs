namespace PharmaCore.Application.Customers.Dtos;

public sealed record CustomerSaleDto(
    int SaleId,
    short? Status,
    decimal? TotalAmount,
    decimal? Discount,
    DateTime? CreatedAt,
    string? Note);
