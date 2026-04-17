namespace PharmaCore.API.Contracts.Sales;

/// <summary>
/// Request body for creating a new sale.
/// </summary>
/// <param name="CustomerId">Optional customer ID.</param>
/// <param name="Note">Optional note.</param>
/// <param name="Discount">Optional discount amount.</param>
public sealed record CreateSaleRequest(
    int? CustomerId,
    string? Note,
    decimal? Discount);