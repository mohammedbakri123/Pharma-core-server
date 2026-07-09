namespace PharmaCore.Application.Inventory.Requests;

public sealed record GetMedicineMovementsQuery(int MedicineId, int Page = 1, int Limit = 20);