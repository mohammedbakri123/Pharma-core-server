namespace PharmaCore.Application.SalesReturn.Requests;

public sealed record AddSalesReturnItemCommand(int SalesReturnId, int SaleItemId, int Quantity, decimal? UnitPrice);