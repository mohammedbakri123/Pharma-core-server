using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class DeletePurchaseReturnService(
    IPurchaseReturnRepository purchaseReturnRepository,
    ILogger<DeletePurchaseReturnService> logger)
    : IDeletePurchaseReturnService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await purchaseReturnRepository.SoftDeleteAsync(purchaseReturnId, cancellationToken);
            if (!success)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Purchase return not found.");

            logger.LogInformation("Deleted purchase return {PurchaseReturnId}", purchaseReturnId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting purchase return {PurchaseReturnId}", purchaseReturnId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}
