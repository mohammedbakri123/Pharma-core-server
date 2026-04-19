using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Sales.Dtos;

public sealed record SaleDto(
    int SaleId,
    int? UserId,
    int? CustomerId,
    SaleStatus Status,
    decimal TotalAmount,
    decimal Discount,
    DateTime CreatedAt,
    string? Note);