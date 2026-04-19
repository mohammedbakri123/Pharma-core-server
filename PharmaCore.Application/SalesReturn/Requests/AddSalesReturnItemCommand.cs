namespace PharmaCore.Application.SalesReturn.Requests;

public sealed record AddSalesReturnItemCommand(int SalesReturnId, int SaleItemId, int BatchId, int Quantity, decimal? UnitPrice);