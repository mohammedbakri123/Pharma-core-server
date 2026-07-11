using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Application.Reports.Interfaces;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Services;

public sealed class GetDashboardReportService(
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository,
    IBatchRepository batchRepository,
    ILogger<GetDashboardReportService> logger)
    : IGetDashboardReportService
{
    private const int MaxRangeDays = 90;
    private const int AlertPreviewLimit = 6;
    private const int RecentLimit = 6;
    private const int LowStockThreshold = 10;
    private const int ExpiringDays = 30;

    public async Task<ServiceResult<DashboardReportDto>> ExecuteAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var fromDate = (from ?? today.AddDays(-6)).Date;
            var toDate = (to ?? today).Date;

            if (fromDate > toDate)
                return ServiceResult<DashboardReportDto>.Fail(ServiceErrorType.Validation, "From date cannot be later than to date.");

            if ((toDate - fromDate).TotalDays + 1 > MaxRangeDays)
                return ServiceResult<DashboardReportDto>.Fail(ServiceErrorType.Validation, $"Dashboard range cannot exceed {MaxRangeDays} days.");

            var allSales = (await saleRepository.ListDetailsAsync(cancellationToken))
                .Where(s => !s.IsDeleted && s.Status == SaleStatus.COMPLETED)
                .ToList();

            var rangeSales = allSales
                .Where(s => s.CreatedAt.Date >= fromDate && s.CreatedAt.Date <= toDate)
                .ToList();

            var rangeSaleIds = rangeSales.Select(s => s.SaleId).ToList();
            var salePayments = (await paymentRepository.GetByReferencesAsync(
                    PaymentReferenceType.SALE,
                    rangeSaleIds,
                    cancellationToken))
                .Where(p => p.Type == PaymentType.INCOMING)
                .ToList();

            var paymentsOverview = await paymentRepository.GetOverviewAsync(
                new ListPaymentsQuery(1, RecentLimit, null, null, null, fromDate, toDate),
                cancellationToken);

            var lowStock = await batchRepository.GetStockAlertsAsync(
                LowStockThreshold,
                null,
                null,
                1,
                AlertPreviewLimit,
                excludeZeroStock: true,
                cancellationToken);

            var expiring = await batchRepository.GetStockAlertsAsync(
                null,
                ExpiringDays,
                null,
                1,
                AlertPreviewLimit,
                excludeZeroStock: true,
                cancellationToken);

            var recentSales = await saleRepository.ListPagedAsync(
                null,
                null,
                SaleStatus.COMPLETED,
                null,
                null,
                1,
                RecentLimit,
                cancellationToken);

            var salesSummary = BuildSalesSummary(rangeSales, salePayments);
            var dailySales = BuildDailySales(fromDate, toDate, rangeSales);
            var recentSaleDtos = recentSales.Items.Select(MapSale).ToList();

            var dashboard = new DashboardReportDto(
                ToOffset(fromDate),
                ToOffset(toDate),
                salesSummary,
                new DashboardCashflowSummaryDto(
                    paymentsOverview.Summary.TotalIn,
                    paymentsOverview.Summary.TotalOut,
                    paymentsOverview.Summary.Net,
                    paymentsOverview.Summary.Cash.In,
                    paymentsOverview.Summary.Cash.Out,
                    paymentsOverview.Summary.Cash.Net,
                    paymentsOverview.Summary.Card.In,
                    paymentsOverview.Summary.Card.Out,
                    paymentsOverview.Summary.Card.Net),
                new DashboardInventorySummaryDto(
                    lowStock.Total,
                    expiring.Total,
                    lowStock.Items,
                    expiring.Items),
                dailySales,
                recentSaleDtos,
                paymentsOverview.Payments);

            return ServiceResult<DashboardReportDto>.Ok(dashboard);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating dashboard report");
            return ServiceResult<DashboardReportDto>.Fail(ServiceErrorType.ServerError, $"Error generating dashboard report: {e.Message}");
        }
    }

    private static DashboardSalesSummaryDto BuildSalesSummary(
        IReadOnlyList<Sale> sales,
        IReadOnlyList<Payment> salePayments)
    {
        var grossRevenue = sales.Sum(s => s.TotalAmount);
        var totalDiscount = sales.Sum(s => s.Discount);
        var netRevenue = grossRevenue - totalDiscount;
        var cashSales = salePayments
            .Where(p => p.Method == PaymentMethod.CASH)
            .Sum(p => p.Amount);
        var cardSales = salePayments
            .Where(p => p.Method == PaymentMethod.CARD)
            .Sum(p => p.Amount);
        var creditSales = Math.Max(0, netRevenue - cashSales - cardSales);

        return new DashboardSalesSummaryDto(
            sales.Count,
            grossRevenue,
            totalDiscount,
            netRevenue,
            sales.Count == 0 ? 0 : netRevenue / sales.Count,
            cashSales,
            cardSales,
            creditSales);
    }

    private static IReadOnlyList<DashboardDailySalesDto> BuildDailySales(
        DateTime from,
        DateTime to,
        IReadOnlyList<Sale> sales)
    {
        var byDate = sales
            .GroupBy(s => s.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var days = (to - from).Days + 1;
        return Enumerable.Range(0, days)
            .Select(offset =>
            {
                var date = from.AddDays(offset);
                var daySales = byDate.GetValueOrDefault(date) ?? new List<Sale>();
                var grossRevenue = daySales.Sum(s => s.TotalAmount);
                var discount = daySales.Sum(s => s.Discount);

                return new DashboardDailySalesDto(
                    ToOffset(date),
                    daySales.Count,
                    grossRevenue,
                    grossRevenue - discount);
            })
            .ToList();
    }

    private static SaleListItemDto MapSale(Sale sale) => new(
        sale.SaleId,
        sale.UserId,
        sale.UserName,
        sale.CustomerId,
        sale.CustomerName,
        sale.Status,
        sale.TotalAmount,
        sale.Discount,
        sale.CreatedAt,
        sale.Note);

    private static DateTimeOffset ToOffset(DateTime date) =>
        new(DateTime.SpecifyKind(date, DateTimeKind.Utc));
}
