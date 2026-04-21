namespace PharmaCore.Application.SalesReturn.Requests;

public sealed record UpdateSalesReturnCommand(int SalesReturnId, string? Note);