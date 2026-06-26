using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class GetPurchaseReturnBalanceService(
    IPurchaseReturnRepository purchaseReturnRepository,
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

            var paidAmount = await paymentRepository.GetTotalAmountByReferenceAsync(PaymentReferenceType.PURCHASE_RETURN, purchaseReturnId, cancellationToken);
            var balance = new PurchaseReturnBalanceDto(
                purchaseReturn.PurchaseReturnId,
                purchaseReturn.TotalAmount,
                paidAmount,
                purchaseReturn.TotalAmount - paidAmount);

            return ServiceResult<PurchaseReturnBalanceDto>.Ok(balance);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting purchase return balance for {PurchaseReturnId}", purchaseReturnId);
            return ServiceResult<PurchaseReturnBalanceDto>.Fail(ServiceErrorType.ServerError, $"Error getting purchase return balance: {e.Message}");
        }
    }
}
