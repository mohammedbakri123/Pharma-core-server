namespace PharmaCore.API.Contracts.Purchases;

public sealed record CreatePurchaseRequest(
    int? SupplierId,
    string? InvoiceNumber,
    string? Note
);
