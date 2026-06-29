using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.SalesReturn.Dtos;

public sealed record SalesReturnDto(
    int SalesReturnId,
    int? SaleId,
    int? CustomerId,
    int? UserId,
    string? UserName,
    SalesReturnStatus Status,
    decimal TotalAmount,
    string? Note,
    DateTime CreatedAt);