namespace PharmaCore.Application.SalesReturn.Dtos;

public sealed record SalesReturnDto(
    int SalesReturnId,
    int? SaleId,
    int? CustomerId,
    int? UserId,
    decimal TotalAmount,
    string? Note,
    DateTime CreatedAt);