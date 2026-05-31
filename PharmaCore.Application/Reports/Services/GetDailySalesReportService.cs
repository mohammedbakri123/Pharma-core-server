using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Application.Reports.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Services;

public class GetDailySalesReportService(
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository,
    ILogger<GetDailySalesReportService> logger)
    : IGetDailySalesReportService
{
    public async Task<ServiceResult<DailySalesReportDto>> ExecuteAsync(DateTime? date, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;

            var allSales = await saleRepository.ListDetailsAsync(cancellationToken);
            var dailySales = allSales
                .Where(s => s.CreatedAt.Date == targetDate && !s.IsDeleted)
                .ToList();

            var totalSales = dailySales.Count;
            var totalRevenue = dailySales.Sum(s => s.TotalAmount);
            var totalDiscount = dailySales.Sum(s => s.Discount);
            var netRevenue = totalRevenue - totalDiscount;

            var saleIds = dailySales.Select(s => s.SaleId).ToList();
            var payments = (await paymentRepository.GetByReferencesAsync(
                PaymentReferenceType.SALE, saleIds, cancellationToken)).ToList();

            var cashSales = payments
                .Where(p => p.Method == PaymentMethod.CASH && p.Type == PaymentType.INCOMING)
                .Sum(p => p.Amount);
            var cardSales = payments
                .Where(p => p.Method == PaymentMethod.CARD && p.Type == PaymentType.INCOMING)
                .Sum(p => p.Amount);
            var creditSales = netRevenue - cashSales - cardSales;

            return ServiceResult<DailySalesReportDto>.Ok(new DailySalesReportDto(
                targetDate, totalSales, totalRevenue, totalDiscount, netRevenue,
                cashSales, cardSales, creditSales > 0 ? creditSales : 0));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating daily sales report for {Date}", date);
            return ServiceResult<DailySalesReportDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
