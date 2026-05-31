namespace PharmaCore.Application.Inventory.Dtos;

public sealed record AdjustmentDto(
    int AdjustmentId,
    int MedicineId,
    int BatchId,
    int Quantity,
    int Type,
    string Reason,
    int UserId,
    DateTime CreatedAt);