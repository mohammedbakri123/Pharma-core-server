using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class DeletePurchaseService(IPurchaseRepository purchaseRepository, ILogger<DeletePurchaseService> logger)
    : IDeletePurchaseService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await purchaseRepository.GetByIdAsync(purchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {purchaseId} not found.");
            }

            var deleted = await purchaseRepository.SoftDeleteAsync(purchaseId, cancellationToken);

            if (deleted)
            {
                logger.LogInformation("Purchase {PurchaseId} soft-deleted successfully", purchaseId);
                return ServiceResult<bool>.Ok(true);
            }

            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to delete purchase.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting purchase {PurchaseId}", purchaseId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error deleting purchase: {e.Message}");
        }
    }
}
