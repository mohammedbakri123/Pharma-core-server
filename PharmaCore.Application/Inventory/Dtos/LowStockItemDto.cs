namespace PharmaCore.Application.Inventory.Dtos;

public sealed record LowStockItemDto(
    int MedicineId,
    string MedicineName,
    string? Barcode,
    int CurrentStock);