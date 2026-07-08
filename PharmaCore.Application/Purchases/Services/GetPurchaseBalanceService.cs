using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Services;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class GetPurchaseBalanceService(
    IPurchaseRepository purchaseRepository,
    IPaymentRepository paymentRepository,
    IPurchaseReturnRepository purchaseReturnRepository,
    ILogger<GetPurchaseBalanceService> logger)
    : IGetPurchaseBalanceService
{
    public async Task<ServiceResult<PurchaseBalanceDto>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await purchaseRepository.GetByIdAsync(purchaseId, cancellationToken);
            if (purchase is null)
                return ServiceResult<PurchaseBalanceDto>.Fail(ServiceErrorType.NotFound, "Purchase not found.");

            var paidAmount = await paymentRepository.GetTotalAmountByReferenceAsync(
                PaymentReferenceType.PURCHASE, purchaseId, cancellationToken);

            var returnedAmount = await purchaseReturnRepository.GetTotalAmountByPurchaseIdAsync(purchaseId, cancellationToken);
            var remainingAmount = Math.Max(0, PaymentCalculations.ComputePurchaseRemaining(purchase.TotalAmount, paidAmount, returnedAmount));
            var refundedAmount = await paymentRepository.GetTotalRefundedByPurchaseIdAsync(purchaseId, cancellationToken);

            return ServiceResult<PurchaseBalanceDto>.Ok(new PurchaseBalanceDto(
                purchase.PurchaseId,
                purchase.TotalAmount,
                paidAmount,
                returnedAmount,
                refundedAmount,
                remainingAmount));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting balance for purchase {PurchaseId}", purchaseId);
            return ServiceResult<PurchaseBalanceDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
