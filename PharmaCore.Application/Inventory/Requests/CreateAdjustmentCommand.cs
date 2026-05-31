namespace PharmaCore.Application.Inventory.Requests;

public record CreateAdjustmentCommand(
    int MedicineId,
    int BatchId,
    int Quantity,
    int Type,
    int UserId,
    string Reason);
