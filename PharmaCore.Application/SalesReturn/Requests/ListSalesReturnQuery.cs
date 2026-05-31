namespace PharmaCore.Application.SalesReturn.Requests;

public sealed record ListSalesReturnQuery(int Page, int Limit, int? SaleId, int? CustomerId, int? UserId, DateTime? From, DateTime? To);