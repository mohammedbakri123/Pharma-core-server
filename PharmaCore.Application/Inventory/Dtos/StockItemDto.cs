namespace PharmaCore.Application.Inventory.Dtos;

public sealed record StockItemDto(
    int MedicineId,
    string MedicineName,
    string? Barcode,
    string Unit,
    int TotalStock);