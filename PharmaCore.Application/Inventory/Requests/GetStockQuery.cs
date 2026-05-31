namespace PharmaCore.Application.Inventory.Requests;

public record GetStockQuery(int Page, int Limit, int? MedicineId);
