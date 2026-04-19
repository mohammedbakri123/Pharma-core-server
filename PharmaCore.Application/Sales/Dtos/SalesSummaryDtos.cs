namespace PharmaCore.Application.Sales.Dtos;

public sealed record SalesSummaryDto(
    int CustomerId,
    decimal TotalSales,
    decimal TotalPaid,
    decimal TotalReturns,
    decimal NetBalance);

public sealed record StatementEntryDto(
    DateTime Date,
    string Type,
    int ReferenceId,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance);

public sealed record SalesStatementDto(
    int CustomerId,
    IReadOnlyList<StatementEntryDto> Entries,
    decimal OpeningBalance,
    decimal ClosingBalance);
