namespace PharmaCore.Application.Purchases.Dtos;

public sealed record PurchaseItemDetailsDto(
    int PurchaseItemId,
    int MedicineId,
    string? MedicineName,
    int? BatchId,
    string? BatchNumber,
    int Quantity,
    decimal PurchasePrice,
    decimal SellPrice,
    decimal TotalPrice,
    DateOnly? ExpireDate);
