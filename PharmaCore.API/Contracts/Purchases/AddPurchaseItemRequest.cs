namespace PharmaCore.API.Contracts.Purchases;

public sealed record AddPurchaseItemRequest(
    int MedicineId,
    int BatchId,
    int Quantity,
    decimal PurchasePrice,
    decimal SellPrice,
    DateOnly? ExpireDate
);
