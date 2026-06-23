namespace PharmaCore.API.Contracts.Sales;

/// <summary>
/// Request body for adding an item to a sale.
/// </summary>
/// <param name="MedicineId">Medicine ID.</param>
/// <param name="Quantity">Quantity.</param>
public sealed record AddSaleItemRequest(
    int MedicineId,
    int Quantity);