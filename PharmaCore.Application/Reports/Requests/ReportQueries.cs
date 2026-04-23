namespace PharmaCore.Application.Reports.Requests;

public sealed record DailySalesReportQuery(DateTime? Date);
public sealed record SalesRangeReportQuery(DateTime From, DateTime To);
public sealed record ProfitReportQuery(DateTime? From, DateTime? To);
public sealed record StockReportQuery(int? CategoryId);
public sealed record ExpiredItemsReportQuery(DateTime? IncludeExpiredBefore);
public sealed record PaymentsReportQuery(DateTime? From, DateTime? To);
