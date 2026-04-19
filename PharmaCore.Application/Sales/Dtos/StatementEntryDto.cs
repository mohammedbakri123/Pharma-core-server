namespace PharmaCore.Application.Sales.Dtos;

public sealed record StatementEntryDto(
    DateTime Date,
    string Type,
    int ReferenceId,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance);