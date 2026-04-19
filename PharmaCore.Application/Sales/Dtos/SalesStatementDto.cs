namespace PharmaCore.Application.Sales.Dtos;

public sealed record SalesStatementDto(
    int CustomerId,
    IReadOnlyList<StatementEntryDto> Entries,
    decimal OpeningBalance,
    decimal ClosingBalance);