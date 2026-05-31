namespace PharmaCore.Application.SalesReturn.Requests;

public sealed record CreateSalesReturnCommand(int? SaleId, int? CustomerId, int? UserId, string? Note);