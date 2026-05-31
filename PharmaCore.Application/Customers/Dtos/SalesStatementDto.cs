namespace PharmaCore.Application.Customers.Interfaces;

public sealed record SalesStatementDto(
    int CustomerId,
    IReadOnlyList<StatementEntryDto> Entries,
    decimal OpeningBalance,
    decimal ClosingBalance);