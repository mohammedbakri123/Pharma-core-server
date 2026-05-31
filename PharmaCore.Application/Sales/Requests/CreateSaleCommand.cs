namespace PharmaCore.Application.Sales.Requests;

public sealed record CreateSaleCommand(int? UserId, int? CustomerId, string? Note, decimal? Discount);