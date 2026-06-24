using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Inventory.Dtos;

public sealed record StockAlertDto(
    int MedicineId,
    string Name,
    string? ArabicName,
    string? Barcode,
    string? CategoryName,
    MedicineUnit? Unit,
    int TotalQuantity,
    StockStatus Status,
    DateOnly? NearestExpireDate,
    bool IsExpiring);
