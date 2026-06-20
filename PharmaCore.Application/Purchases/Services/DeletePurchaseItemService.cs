using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class DeletePurchaseItemService(IPurchaseRepository purchaseRepository, ILogger<DeletePurchaseItemService> logger)
    : IDeletePurchaseItemService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int purchaseId, int itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await purchaseRepository.GetItemByIdAsync(itemId, cancellationToken);

            if (item is null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Purchase item with ID {itemId} not found.");
            }

            if (item.PurchaseId != purchaseId)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.Validation, "Purchase item does not belong to the specified purchase.");
            }

            var purchase = await purchaseRepository.GetByIdAsync(purchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {purchaseId} not found.");
            }

            if (purchase.Status != PurchaseStatus.Draft)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.Validation, "Only draft purchases can be modified.");
            }

            var deleted = await purchaseRepository.DeleteItemAsync(itemId, cancellationToken);

            if (!deleted)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to delete purchase item.");
            }

            await purchaseRepository.UpdateTotalAmountAsync(purchaseId, cancellationToken);

            logger.LogInformation("Purchase item {ItemId} deleted from purchase {PurchaseId}", itemId, purchaseId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting purchase item {ItemId}", itemId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error deleting item: {e.Message}");
        }
    }
}
