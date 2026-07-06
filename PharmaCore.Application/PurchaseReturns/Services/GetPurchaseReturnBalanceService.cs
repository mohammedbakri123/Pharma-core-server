using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Services;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class GetPurchaseReturnBalanceService(
    IPurchaseReturnRepository purchaseReturnRepository,
    IPurchaseRepository purchaseRepository,
    IPaymentRepository paymentRepository,
    ILogger<GetPurchaseReturnBalanceService> logger)
    : IGetPurchaseReturnBalanceService
{
    public async Task<ServiceResult<PurchaseReturnBalanceDto>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchaseReturn = await purchaseReturnRepository.GetByIdAsync(purchaseReturnId, cancellationToken);
            if (purchaseReturn is null)
                return ServiceResult<PurchaseReturnBalanceDto>.Fail(ServiceErrorType.NotFound, "Purchase return not found.");

            var paidAmount = await paymentRepository.GetTotalAmountByReferenceAsync(
                PaymentReferenceType.PURCHASE_RETURN, purchaseReturnId, cancellationToken);

            var maxRefundable = await ComputeMaxRefundableAsync(purchaseReturn, cancellationToken);
            var remainingAmount = Math.Max(0, maxRefundable - paidAmount);

            var balance = new PurchaseReturnBalanceDto(
                purchaseReturn.PurchaseReturnId,
                purchaseReturn.TotalAmount,
                paidAmount,
                remainingAmount);

            return ServiceResult<PurchaseReturnBalanceDto>.Ok(balance);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting purchase return balance for {PurchaseReturnId}", purchaseReturnId);
            return ServiceResult<PurchaseReturnBalanceDto>.Fail(ServiceErrorType.ServerError, $"Error getting purchase return balance: {e.Message}");
        }
    }

    private async Task<decimal> ComputeMaxRefundableAsync(Domain.Entities.PurchaseReturn purchaseReturn, CancellationToken cancellationToken)
    {
        var purchase = await purchaseRepository.GetByIdAsync(purchaseReturn.PurchaseId ?? 0, cancellationToken);
        if (purchase is null)
            return 0;

        var totalPaidOnPurchase = await paymentRepository.GetTotalAmountByReferenceAsync(
            PaymentReferenceType.PURCHASE, purchase.PurchaseId, cancellationToken);

        return PaymentCalculations.ComputePurchaseReturnMaxRefund(purchase.TotalAmount, purchaseReturn.TotalAmount, totalPaidOnPurchase);
    }
}
