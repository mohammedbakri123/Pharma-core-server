namespace PharmaCore.API.Contracts.Inventory;

/// <summary>
/// Request body for creating a stock adjustment.
/// </summary>
/// <param name="MedicineId">Medicine ID.</param>
/// <param name="BatchId">Batch ID.</param>
/// <param name="Quantity">Quantity to adjust.</param>
/// <param name="Type">Adjustment type (1: INCREASE, 2: DECREASE).</param>
/// <param name="UserId">User ID performing the adjustment.</param>
/// <param name="Reason">Reason for adjustment.</param>
public sealed record AdjustmentRequest(
    int MedicineId,
    int BatchId,
    int Quantity,
    int Type,
    int UserId,
    string Reason);
