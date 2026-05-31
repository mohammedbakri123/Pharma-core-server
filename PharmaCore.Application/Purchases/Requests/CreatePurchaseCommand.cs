namespace PharmaCore.Application.Purchases.Requests;

public sealed record CreatePurchaseCommand(
    int? SupplierId,
    string? InvoiceNumber,
    string? Note
);
