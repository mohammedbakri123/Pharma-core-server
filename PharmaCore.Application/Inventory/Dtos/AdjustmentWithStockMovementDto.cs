namespace PharmaCore.Application.Inventory.Dtos;

public sealed record AdjustmentWithStockMovementDto(
    int AdjustmentId,
    int MedicineId,
    int BatchId,
    int Quantity,
    int Type,
    string Reason,
    int UserId,
    DateTime CreatedAt,
    StockMovementDto StockMovement);