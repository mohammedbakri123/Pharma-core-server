using System.Collections.Generic;

namespace PharmaCore.Application.Reports.Dtos;

public sealed record SalesRangeReportDto(
    DateTime From,
    DateTime To,
    int TotalSales,
    decimal TotalRevenue,
    decimal TotalDiscount,
    decimal NetRevenue,
    decimal CashSales,
    decimal CardSales,
    decimal CreditSales,
    IReadOnlyList<DailyBreakdownDto> DailyBreakdown);
