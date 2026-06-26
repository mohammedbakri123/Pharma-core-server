using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class CancelPurchaseReturnService(
    IPurchaseReturnRepository purchaseReturnRepository,
    ILogger<CancelPurchaseReturnService> logger)
    : ICancelPurchaseReturnService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchaseReturn = await purchaseReturnRepository.GetByIdAsync(purchaseReturnId, cancellationToken);
            if (purchaseReturn is null)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Purchase return not found.");

            if (purchaseReturn.Status != PurchaseReturnStatus.DRAFT)
                return ServiceResult<bool>.Fail(ServiceErrorType.Validation, "Only draft purchase returns can be cancelled.");

            purchaseReturn.Cancel();
            await purchaseReturnRepository.UpdateAsync(purchaseReturn, cancellationToken);

            logger.LogInformation("Cancelled purchase return {PurchaseReturnId}", purchaseReturnId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error cancelling purchase return {PurchaseReturnId}", purchaseReturnId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error cancelling purchase return: {e.Message}");
        }
    }
}
