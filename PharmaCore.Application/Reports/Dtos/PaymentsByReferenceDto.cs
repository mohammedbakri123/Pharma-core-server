namespace PharmaCore.Application.Reports.Dtos;

public sealed record PaymentsByReferenceDto(
    decimal Sales,
    decimal Purchases,
    decimal Expenses,
    decimal Returns);
