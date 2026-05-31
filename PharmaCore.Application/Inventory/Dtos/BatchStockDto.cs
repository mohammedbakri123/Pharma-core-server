namespace PharmaCore.Application.Inventory.Dtos;

public sealed record BatchStockDto(
    int BatchId,
    string? BatchNumber,
    int QuantityRemaining,
    decimal PurchasePrice,
    decimal SellPrice,
    DateOnly? ExpireDate);