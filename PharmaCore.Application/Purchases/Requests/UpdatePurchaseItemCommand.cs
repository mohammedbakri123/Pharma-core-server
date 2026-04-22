namespace PharmaCore.Application.Purchases.Requests;

public sealed record UpdatePurchaseItemCommand(
    int PurchaseId,
    int ItemId,
    int? Quantity,
    decimal? PurchasePrice,
    decimal? SellPrice,
    DateOnly? ExpireDate
);
