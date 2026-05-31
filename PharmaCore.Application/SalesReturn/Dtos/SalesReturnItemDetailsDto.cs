namespace PharmaCore.Application.SalesReturn.Dtos;

public sealed record SalesReturnItemDetailsDto(
    int SalesReturnItemId,
    int SaleItemId,
    int BatchId,
    string? BatchNumber,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);