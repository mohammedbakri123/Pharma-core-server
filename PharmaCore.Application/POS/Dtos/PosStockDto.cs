namespace PharmaCore.Application.POS.Dtos;

public sealed record PosStockDto(
    int MedicineId,
    string Name,
    int TotalStock,
    List<PosBatchDto> Batches
);

public sealed record PosBatchDto(
    int BatchId,
    string? BatchNumber,
    int QuantityRemaining,
    decimal SellPrice,
    DateOnly? ExpireDate
);
