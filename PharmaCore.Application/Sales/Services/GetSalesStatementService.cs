// using PharmaCore.Application.Abstractions.Persistence;
// using PharmaCore.Application.Sales.Dtos;
// using PharmaCore.Application.Sales.Interfaces;
// using PharmaCore.Domain.Enums;
// using PharmaCore.Domain.Shared;
//
// namespace PharmaCore.Application.Sales.Services;
//
// public class GetSalesStatementService(
//     ISaleRepository saleRepository,
//     IPaymentRepository paymentRepository,
//     ISalesReturnRepository salesReturnRepository)
//     : IGetSalesStatementService
// {
//     public async Task<ServiceResult<SalesStatementDto>> ExecuteAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
//     {
//         var entries = new List<StatementEntryDto>();
//
//         // 1. Get Sales
//         var sales = await saleRepository.GetByCustomerIdAsync(customerId, from, to, cancellationToken);
//         foreach (var sale in sales)
//         {
//             entries.Add(new StatementEntryDto(
//                 sale.CreatedAt,
//                 "SALE",
//                 sale.SaleId,
//                 $"Sale #{sale.SaleId}",
//                 sale.TotalAmount,
//                 0m,
//                 0m));
//         }
//
//         // 2. Get Payments (requires sale IDs)
//         var allSales = await saleRepository.GetByCustomerIdAsync(customerId, null, null, cancellationToken);
//         var saleIds = allSales.Select(s => s.SaleId).ToList();
//
//         foreach (var saleId in saleIds)
//         {
//             var paymentsResult = await paymentRepository.GetByReferenceAsync(PaymentReferenceType.SALE, saleId, cancellationToken);
//             foreach (var payment in paymentsResult.Payments)
//             {
//                 if ((!from.HasValue || payment.CreatedAt >= from) && (!to.HasValue || payment.CreatedAt <= to))
//                 {
//                     entries.Add(new StatementEntryDto(
//                         payment.CreatedAt,
//                         "PAYMENT",
//                         payment.PaymentId,
//                         payment.Description ?? $"Payment for Sale #{saleId}",
//                         0m,
//                         payment.Amount,
//                         0m));
//                 }
//             }
//         }
//
//         // 3. Get Returns
//         var returns = await salesReturnRepository.GetByCustomerIdAsync(customerId, from, to, cancellationToken);
//         foreach (var ret in returns)
//         {
//             entries.Add(new StatementEntryDto(
//                 ret.CreatedAt ?? DateTime.UtcNow,
//                 "RETURN",
//                 ret.SalesReturnId,
//                 ret.Note ?? $"Sales Return #{ret.SalesReturnId}",
//                 0m,
//                 ret.TotalAmount ?? 0m,
//                 0m));
//         }
//
//         // Calculate running balance
//         decimal runningBalance = 0;
//         var orderedEntries = entries.OrderBy(e => e.Date).ToList();
//         var finalEntries = new List<StatementEntryDto>();
//
//         foreach (var entry in orderedEntries)
//         {
//             runningBalance += entry.Debit - entry.Credit;
//             finalEntries.Add(entry with { RunningBalance = runningBalance });
//         }
//
//         var result = new SalesStatementDto(
//             customerId,
//             finalEntries,
//             0m,
//             runningBalance);
//
//         return ServiceResult<SalesStatementDto>.Ok(result);
//     }
// }
