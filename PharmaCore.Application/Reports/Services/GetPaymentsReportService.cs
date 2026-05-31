using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Application.Reports.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Services;

public class GetPaymentsReportService(
    IPaymentRepository paymentRepository,
    ILogger<GetPaymentsReportService> logger)
    : IGetPaymentsReportService
{
    public async Task<ServiceResult<PaymentsReportDto>> ExecuteAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        try
        {
            var allPayments = (await paymentRepository.ListAsync(cancellationToken)).ToList();
            var filtered = allPayments.AsEnumerable();

            if (from.HasValue)
                filtered = filtered.Where(p => p.CreatedAt.HasValue && p.CreatedAt.Value.Date >= from.Value.Date);
            if (to.HasValue)
                filtered = filtered.Where(p => p.CreatedAt.HasValue && p.CreatedAt.Value.Date <= to.Value.Date);

            var paymentsList = filtered.ToList();

            var totalIn = paymentsList.Where(p => p.Type == PaymentType.INCOMING).Sum(p => p.Amount);
            var totalOut = paymentsList.Where(p => p.Type == PaymentType.OUTGOING).Sum(p => p.Amount);

            var cashIn = paymentsList
                .Where(p => p.Method == PaymentMethod.CASH && p.Type == PaymentType.INCOMING)
                .Sum(p => p.Amount);
            var cashOut = paymentsList
                .Where(p => p.Method == PaymentMethod.CASH && p.Type == PaymentType.OUTGOING)
                .Sum(p => p.Amount);
            var cardIn = paymentsList
                .Where(p => p.Method == PaymentMethod.CARD && p.Type == PaymentType.INCOMING)
                .Sum(p => p.Amount);
            var cardOut = paymentsList
                .Where(p => p.Method == PaymentMethod.CARD && p.Type == PaymentType.OUTGOING)
                .Sum(p => p.Amount);

            var salesPayments = paymentsList
                .Where(p => p.ReferenceType == PaymentReferenceType.SALE)
                .Sum(p => p.Amount);
            var purchasePayments = paymentsList
                .Where(p => p.ReferenceType == PaymentReferenceType.PURCHASE)
                .Sum(p => p.Amount);
            var expensePayments = paymentsList
                .Where(p => p.ReferenceType == PaymentReferenceType.EXPENSE)
                .Sum(p => p.Amount);
            var returnPayments = paymentsList
                .Where(p => p.ReferenceType == PaymentReferenceType.PURCHASE_RETURN)
                .Sum(p => p.Amount);

            return ServiceResult<PaymentsReportDto>.Ok(new PaymentsReportDto(
                from, to, totalIn, totalOut,
                new PaymentMethodSummaryDto(
                    new PaymentSummaryDto(cashIn, cashOut, cashIn - cashOut),
                    new PaymentSummaryDto(cardIn, cardOut, cardIn - cardOut)),
                new PaymentsByReferenceDto(
                    salesPayments, purchasePayments, expensePayments, returnPayments)));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating payments report");
            return ServiceResult<PaymentsReportDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
