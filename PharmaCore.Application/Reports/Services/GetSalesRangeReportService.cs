using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Application.Reports.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Services;

public class GetSalesRangeReportService(
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository,
    ILogger<GetSalesRangeReportService> logger)
    : IGetSalesRangeReportService
{
    public async Task<ServiceResult<SalesRangeReportDto>> ExecuteAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        try
        {
            var allSales = await saleRepository.ListDetailsAsync(cancellationToken);
            var rangeSales = allSales
                .Where(s => s.CreatedAt.Date >= from.Date && s.CreatedAt.Date <= to.Date && !s.IsDeleted)
                .ToList();

            var totalSales = rangeSales.Count;
            var totalRevenue = rangeSales.Sum(s => s.TotalAmount);
            var totalDiscount = rangeSales.Sum(s => s.Discount);
            var netRevenue = totalRevenue - totalDiscount;

            var saleIds = rangeSales.Select(s => s.SaleId).ToList();
            var payments = (await paymentRepository.GetByReferencesAsync(
                PaymentReferenceType.SALE, saleIds, cancellationToken)).ToList();

            var cashSales = payments
                .Where(p => p.Method == PaymentMethod.CASH && p.Type == PaymentType.INCOMING)
                .Sum(p => p.Amount);
            var cardSales = payments
                .Where(p => p.Method == PaymentMethod.CARD && p.Type == PaymentType.INCOMING)
                .Sum(p => p.Amount);
            var creditSales = netRevenue - cashSales - cardSales;

            var dailyBreakdown = rangeSales
                .GroupBy(s => s.CreatedAt.Date)
                .Select(g => new DailyBreakdownDto(
                    g.Key, g.Count(), g.Sum(s => s.TotalAmount), g.Sum(s => s.Discount)))
                .OrderBy(d => d.Date)
                .ToList();

            return ServiceResult<SalesRangeReportDto>.Ok(new SalesRangeReportDto(
                from, to, totalSales, totalRevenue, totalDiscount, netRevenue,
                cashSales, cardSales, creditSales > 0 ? creditSales : 0,
                dailyBreakdown));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating sales range report for {From} to {To}", from, to);
            return ServiceResult<SalesRangeReportDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
