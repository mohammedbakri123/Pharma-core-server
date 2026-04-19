using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Sales.Dtos;

public sealed record SaleListItemDto(
    int SaleId,
    int? UserId,
    string? UserName,
    int? CustomerId,
    string? CustomerName,
    SaleStatus Status,
    decimal TotalAmount,
    decimal Discount,
    DateTime CreatedAt,
    string? Note);