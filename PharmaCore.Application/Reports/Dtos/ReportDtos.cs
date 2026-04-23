namespace PharmaCore.Application.Reports.Dtos;

public sealed record DailySalesReportDto(
    DateTime Date,
    int TotalSales,
    decimal TotalRevenue,
    decimal TotalDiscount,
    decimal NetRevenue,
    decimal CashSales,
    decimal CardSales,
    decimal CreditSales
);

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
    List<DailyBreakdownDto> DailyBreakdown
);

public sealed record DailyBreakdownDto(
    DateTime Date,
    int SalesCount,
    decimal Revenue,
    decimal Discount
);

public sealed record ProfitReportDto(
    DateTime? From,
    DateTime? To,
    decimal TotalSalesRevenue,
    decimal TotalCost,
    decimal GrossProfit,
    decimal TotalExpenses,
    decimal TotalReturns,
    decimal NetProfit,
    decimal ProfitMargin
);

public sealed record StockReportDto(
    int TotalMedicines,
    int TotalBatches,
    decimal TotalStockValue,
    List<StockByCategoryDto> StockByCategory,
    int LowStockCount,
    int ExpiredCount,
    int ExpiringSoonCount
);

public sealed record StockByCategoryDto(
    int CategoryId,
    string CategoryName,
    int TotalStock,
    decimal StockValue
);

public sealed record ExpiredItemsReportDto(
    List<ExpiredItemDto> ExpiredItems,
    decimal TotalExpiredValue
);

public sealed record ExpiredItemDto(
    int BatchId,
    int MedicineId,
    string MedicineName,
    string? BatchNumber,
    int QuantityRemaining,
    DateOnly? ExpireDate,
    decimal StockValue
);

public sealed record PaymentsReportDto(
    DateTime? From,
    DateTime? To,
    decimal TotalIn,
    decimal TotalOut,
    PaymentMethodSummaryDto ByMethod,
    Dictionary<string, decimal> ByReferenceType
);

public sealed record PaymentMethodSummaryDto(
    PaymentMethodDetailDto Cash,
    PaymentMethodDetailDto Card
);

public sealed record PaymentMethodDetailDto(
    decimal In,
    decimal Out,
    decimal Net
);
