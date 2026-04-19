namespace PharmaCore.Application.Sales.Dtos;

public sealed record SalesSummaryDto(
    int CustomerId,
    decimal TotalSales,
    decimal TotalPaid,
    decimal TotalReturns,
    decimal NetBalance);