namespace PharmaCore.Application.SalesReturn.Dtos;

public sealed record SalesReturnListItemDto(
    int SalesReturnId,
    int? SaleId,
    string? SaleNumber,
    int? CustomerId,
    string? CustomerName,
    int? UserId,
    string? UserName,
    decimal TotalAmount,
    string? Note,
    DateTime CreatedAt);