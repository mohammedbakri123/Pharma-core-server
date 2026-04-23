using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Application.Reports.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Services;

public class GetProfitReportService(
    ISaleRepository saleRepository,
    IPurchaseRepository purchaseRepository,
    IExpenseRepository expenseRepository,
    IPaymentRepository paymentRepository,
    ILogger<GetProfitReportService> logger)
    : IGetProfitReportService
{
    public async Task<ServiceResult<ProfitReportDto>> ExecuteAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        try
        {
            var allSales = await saleRepository.ListDetailsAsync(cancellationToken);
            var filteredSales = allSales.Where(s => s.IsDeleted == false);

            if (from.HasValue)
                filteredSales = filteredSales.Where(s => s.CreatedAt.Date >= from.Value.Date);
            if (to.HasValue)
                filteredSales = filteredSales.Where(s => s.CreatedAt.Date <= to.Value.Date);

            var salesList = filteredSales.ToList();
            var totalSalesRevenue = salesList.Sum(s => s.TotalAmount);

            // Calculate cost of goods sold (using purchase price from sale items)
            decimal totalCost = 0;
            foreach (var sale in salesList)
            {
                foreach (var item in sale.Items)
                {
                    totalCost += item.PurchasePrice * item.Quantity;
                }
            }

            var grossProfit = totalSalesRevenue - totalCost;

            // Get expenses in period
            var allExpenses = await expenseRepository.ListAsync(cancellationToken);
            var expenses = allExpenses.Where(e => e.IsDeleted != true);
            if (from.HasValue)
                expenses = expenses.Where(e => e.CreatedAt.HasValue && e.CreatedAt.Value.Date >= from.Value.Date);
            if (to.HasValue)
                expenses = expenses.Where(e => e.CreatedAt.HasValue && e.CreatedAt.Value.Date <= to.Value.Date);
            var totalExpenses = expenses.Sum(e => e.Amount);

            // Get returns in period
            var saleIds = salesList.Select(s => s.SaleId).ToList();
            var refundPayments = (await paymentRepository.GetByReferencesAsync(
                PaymentReferenceType.SALES_RETURN, saleIds, cancellationToken)).ToList();
            var totalReturns = refundPayments.Sum(p => p.Amount);

            var netProfit = grossProfit - totalExpenses - totalReturns;
            var profitMargin = totalSalesRevenue > 0 ? (netProfit / totalSalesRevenue) * 100 : 0;

            return ServiceResult<ProfitReportDto>.Ok(new ProfitReportDto(
                from, to, totalSalesRevenue, totalCost, grossProfit,
                totalExpenses, totalReturns, netProfit, profitMargin));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating profit report");
            return ServiceResult<ProfitReportDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
