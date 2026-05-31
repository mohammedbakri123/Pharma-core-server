namespace PharmaCore.Application.SalesReturn.Requests;

public sealed record UpdateSalesReturnItemCommand(int SalesReturnItemId, int Quantity);