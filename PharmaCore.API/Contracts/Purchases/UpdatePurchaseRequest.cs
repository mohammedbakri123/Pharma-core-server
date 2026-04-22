namespace PharmaCore.API.Contracts.Purchases;

public sealed record UpdatePurchaseRequest(
    int? SupplierId,
    string? InvoiceNumber,
    string? Note
);
