namespace PharmaCore.API.Contracts.Purchases;

public sealed record UpdatePurchaseItemRequest(
    int? Quantity,
    decimal? PurchasePrice,
    decimal? SellPrice,
    DateOnly? ExpireDate
);
