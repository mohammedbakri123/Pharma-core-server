namespace PharmaCore.Application.Sales.Dtos;

public sealed record SaleItemDetailsDto(
    int SaleItemId,
    int MedicineId,
    string? MedicineName,
    int BatchId,
    string? BatchNumber,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);