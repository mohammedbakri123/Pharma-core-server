using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class GetSalesStatementService(
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository,
    ISalesReturnRepository salesReturnRepository)
    : IGetSalesStatementService
{
    public async Task<ServiceResult<SalesStatementDto>> ExecuteAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        // 1. Calculate Opening Balance (all transactions before 'from')
        decimal openingBalance = 0;
        if (from.HasValue)
        {
            var salesBefore = await saleRepository.GetByCustomerIdAsync(customerId, null, from.Value.AddTicks(-1), cancellationToken);
            var saleIdsBefore = salesBefore.Select(s => s.SaleId).ToList();
            
            var paymentsBefore = await paymentRepository.GetByReferencesAsync(PaymentReferenceType.SALE, saleIdsBefore, cancellationToken);
            var returnsBefore = await salesReturnRepository.GetByCustomerIdAsync(customerId, null, from.Value.AddTicks(-1), cancellationToken);

            openingBalance = salesBefore.Sum(s => s.TotalAmount) 
                             - paymentsBefore.Sum(p => p.Amount) 
                             - returnsBefore.Sum(r => r.TotalAmount);
        }

        var entries = new List<StatementEntryDto>();

        // 2. Fetch Transactions within range
        var salesInRange = await saleRepository.GetByCustomerIdAsync(customerId, from, to, cancellationToken);
        foreach (var sale in salesInRange)
        {
            entries.Add(new StatementEntryDto(
                sale.CreatedAt,
                "SALE",
                sale.SaleId,
                $"Sale #{sale.SaleId}",
                sale.TotalAmount,
                0m,
                0m));
        }

        // Fetch Payments for ALL customer sales that occurred within the date range
        var allCustomerSales = await saleRepository.GetByCustomerIdAsync(customerId, null, null, cancellationToken);
        var allSaleIds = allCustomerSales.Select(s => s.SaleId).ToList();
        
        var payments = await paymentRepository.GetByReferencesAsync(PaymentReferenceType.SALE, allSaleIds, cancellationToken);
        foreach (var payment in payments)
        {
            if ((!from.HasValue || payment.CreatedAt >= from) && (!to.HasValue || payment.CreatedAt <= to))
            {
                entries.Add(new StatementEntryDto(
                    payment.CreatedAt ?? DateTime.UtcNow,
                    "PAYMENT",
                    payment.PaymentId,
                    payment.Description ?? $"Payment for Sale #{payment.ReferenceId}",
                    0m,
                    payment.Amount,
                    0m));
            }
        }

        var returnsInRange = await salesReturnRepository.GetByCustomerIdAsync(customerId, from, to, cancellationToken);
        foreach (var ret in returnsInRange)
        {
            entries.Add(new StatementEntryDto(
                ret.CreatedAt,
                "RETURN",
                ret.SalesReturnId,
                ret.Note ?? $"Sales Return #{ret.SalesReturnId}",
                0m,
                ret.TotalAmount,
                0m));
        }

        // 3. Sort and Calculate Running Balance
        var orderedEntries = entries.OrderBy(e => e.Date).ToList();
        decimal runningBalance = openingBalance;
        var finalEntries = new List<StatementEntryDto>();

        foreach (var entry in orderedEntries)
        {
            runningBalance += entry.Debit - entry.Credit;
            finalEntries.Add(entry with { RunningBalance = runningBalance });
        }

        return ServiceResult<SalesStatementDto>.Ok(new SalesStatementDto(
            customerId,
            finalEntries,
            openingBalance,
            runningBalance));
    }
}
