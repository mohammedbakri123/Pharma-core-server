namespace PharmaCore.Application.Reports.Dtos;

public sealed record ProfitReportDto(
    DateTime? From,
    DateTime? To,
    decimal TotalSalesRevenue,
    decimal TotalCost,
    decimal GrossProfit,
    decimal TotalExpenses,
    decimal TotalReturns,
    decimal NetProfit,
    decimal ProfitMargin);
