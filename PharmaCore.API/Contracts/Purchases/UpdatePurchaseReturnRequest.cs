namespace PharmaCore.API.Contracts.Purchases;

/// <summary>
/// Request body for updating a purchase return.
/// </summary>
/// <param name="Note">Note.</param>
public sealed record UpdatePurchaseReturnRequest(string? Note);
