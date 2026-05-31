namespace PharmaCore.API.Contracts.Sales;

/// <summary>
/// Request body for completing a sale.
/// </summary>
/// <param name="Payments">Optional list of payments.</param>
public sealed record CompleteSaleRequest(IReadOnlyList<SalePaymentRequest>? Payments);