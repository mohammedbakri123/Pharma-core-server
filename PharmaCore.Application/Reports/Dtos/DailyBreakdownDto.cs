namespace PharmaCore.Application.Reports.Dtos;

public sealed record DailyBreakdownDto(
    DateTime Date,
    int SalesCount,
    decimal Revenue,
    decimal Discount);
