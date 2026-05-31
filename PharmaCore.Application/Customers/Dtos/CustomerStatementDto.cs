namespace PharmaCore.Application.Customers.Dtos;

public sealed record StatementEntryDto(
    DateTime? Date,
    string Type,
    int ReferenceId,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance);

public sealed record CustomerStatementDto(
    int CustomerId,
    string CustomerName,
    IReadOnlyList<StatementEntryDto> Statement,
    decimal OpeningBalance,
    decimal ClosingBalance);
