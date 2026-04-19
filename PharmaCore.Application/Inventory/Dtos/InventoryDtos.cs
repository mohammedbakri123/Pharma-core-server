namespace PharmaCore.Application.Inventory.Dtos;

public sealed record StockItemDto(
    int MedicineId,
    string MedicineName,
    string? Barcode,
    string Unit,
    int TotalStock);

public sealed record BatchStockDto(
    int BatchId,
    string? BatchNumber,
    int QuantityRemaining,
    decimal PurchasePrice,
    decimal SellPrice,
    DateOnly? ExpireDate);

public sealed record StockWithBatchesDto(
    int MedicineId,
    string MedicineName,
    int TotalStock,
    IReadOnlyList<BatchStockDto> Batches);

public sealed record LowStockItemDto(
    int MedicineId,
    string MedicineName,
    string? Barcode,
    int CurrentStock);

public sealed record ExpiringItemDto(
    int BatchId,
    int MedicineId,
    string MedicineName,
    string? BatchNumber,
    int QuantityRemaining,
    DateOnly ExpireDate,
    int DaysUntilExpiry);

public sealed record AdjustmentDto(
    int AdjustmentId,
    int MedicineId,
    int BatchId,
    int Quantity,
    int Type,
    string Reason,
    int UserId,
    DateTime CreatedAt);

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

public sealed record StockMovementDto(
    int StockMovementId,
    int MedicineId,
    int BatchId,
    int Quantity,
    int Type,
    int ReferenceType,
    int ReferenceId,
    DateTime CreatedAt);