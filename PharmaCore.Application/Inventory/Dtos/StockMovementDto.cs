namespace PharmaCore.Application.Inventory.Dtos;

public sealed record StockMovementDto(
    int StockMovementId,
    int MedicineId,
    int BatchId,
    int Quantity,
    int Type,
    int ReferenceType,
    int ReferenceId,
    DateTime CreatedAt,
    string? MedicineName = null,
    string? BatchNumber = null);