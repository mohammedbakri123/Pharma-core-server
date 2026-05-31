namespace PharmaCore.Application.Inventory.Dtos;

public sealed record ExpiringItemDto(
    int BatchId,
    int MedicineId,
    string MedicineName,
    string? BatchNumber,
    int QuantityRemaining,
    DateOnly ExpireDate,
    int DaysUntilExpiry);