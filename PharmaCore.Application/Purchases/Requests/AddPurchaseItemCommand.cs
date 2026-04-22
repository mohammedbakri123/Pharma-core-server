namespace PharmaCore.Application.Purchases.Requests;

public sealed record AddPurchaseItemCommand(
    int PurchaseId,
    int MedicineId,
    int BatchId,
    int Quantity,
    decimal PurchasePrice,
    decimal SellPrice,
    DateOnly? ExpireDate
);
