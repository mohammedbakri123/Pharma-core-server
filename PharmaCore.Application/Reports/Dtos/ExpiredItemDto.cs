namespace PharmaCore.Application.Reports.Dtos;

public sealed record ExpiredItemDto(
    int BatchId,
    int MedicineId,
    string MedicineName,
    string BatchNumber,
    int QuantityRemaining,
    DateOnly ExpireDate,
    decimal StockValue);
