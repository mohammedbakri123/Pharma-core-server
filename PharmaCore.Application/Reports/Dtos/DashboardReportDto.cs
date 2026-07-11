using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Sales.Dtos;

namespace PharmaCore.Application.Reports.Dtos;

/// <summary>Aggregated dashboard data for the pharmacy home page.</summary>
public sealed record DashboardReportDto(
    DateTimeOffset From,
    DateTimeOffset To,
    DashboardSalesSummaryDto Sales,
    DashboardCashflowSummaryDto Cashflow,
    DashboardInventorySummaryDto Inventory,
    IReadOnlyList<DashboardDailySalesDto> DailySales,
    IReadOnlyList<SaleListItemDto> RecentSales,
    IReadOnlyList<PaymentOverviewItemDto> RecentPayments);

/// <summary>Sales totals for the selected dashboard period.</summary>
public sealed record DashboardSalesSummaryDto(
    int TotalSales,
    decimal GrossRevenue,
    decimal TotalDiscount,
    decimal NetRevenue,
    decimal AverageSale,
    decimal CashSales,
    decimal CardSales,
    decimal CreditSales);

/// <summary>Cash movement totals for the selected dashboard period.</summary>
public sealed record DashboardCashflowSummaryDto(
    decimal TotalIn,
    decimal TotalOut,
    decimal Net,
    decimal CashIn,
    decimal CashOut,
    decimal CashNet,
    decimal CardIn,
    decimal CardOut,
    decimal CardNet);

/// <summary>Inventory risk counters and highlighted stock alerts.</summary>
public sealed record DashboardInventorySummaryDto(
    int LowStockCount,
    int ExpiringCount,
    IReadOnlyList<StockAlertDto> LowStockItems,
    IReadOnlyList<StockAlertDto> ExpiringItems);

/// <summary>Daily sales totals used by dashboard charts.</summary>
public sealed record DashboardDailySalesDto(
    DateTimeOffset Date,
    int TotalSales,
    decimal GrossRevenue,
    decimal NetRevenue);
