namespace PharmaCore.Application.Sales.Requests;

public sealed record AddSaleItemCommand(int SaleId, int MedicineId, int Quantity, decimal? UnitPrice);