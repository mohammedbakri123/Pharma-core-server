namespace PharmaCore.API.Contracts.Purchases;

public sealed record AddPurchaseItemRequest(
    int MedicineId,
    string BatchNumber,
    int Quantity,
    decimal PurchasePrice,
    decimal SellPrice,
    DateOnly? ExpireDate
);
