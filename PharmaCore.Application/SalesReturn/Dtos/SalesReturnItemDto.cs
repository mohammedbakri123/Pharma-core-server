namespace PharmaCore.Application.SalesReturn.Dtos;

public sealed record SalesReturnItemDto(
    int SalesReturnItemId,
    int SalesReturnId,
    int SaleItemId,
    int BatchId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);