namespace PharmaCore.Application.Inventory.Dtos;

public sealed record StockWithBatchesDto(
    int MedicineId,
    string MedicineName,
    int TotalStock,
    IReadOnlyList<BatchStockDto> Batches);