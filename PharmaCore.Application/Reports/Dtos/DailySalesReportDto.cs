namespace PharmaCore.Application.Reports.Dtos;

public sealed record DailySalesReportDto(
    DateTime Date,
    int TotalSales,
    decimal TotalRevenue,
    decimal TotalDiscount,
    decimal NetRevenue,
    decimal CashSales,
    decimal CardSales,
    decimal CreditSales);
