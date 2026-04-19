namespace PharmaCore.Application.Sales.Dtos;

public sealed record SaleItemDto(
    int SaleItemId,
    int SaleId,
    int MedicineId,
    int BatchId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);