using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Customers.Dtos;

public sealed record CustomerSaleDto(
    int SaleId,
    SaleStatus? Status,
    decimal? TotalAmount,
    decimal? Discount,
    DateTime? CreatedAt,
    string? Note);
