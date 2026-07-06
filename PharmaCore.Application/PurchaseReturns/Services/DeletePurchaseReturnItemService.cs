using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class DeletePurchaseReturnItemService(
    IPurchaseReturnRepository purchaseReturnRepository,
    ILogger<DeletePurchaseReturnItemService> logger)
    : IDeletePurchaseReturnItemService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(
        DeletePurchaseReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await purchaseReturnRepository.GetItemByIdAsync(command.PurchaseReturnItemId, cancellationToken);
            if (item is null)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Purchase return item not found.");

            var purchaseReturn = await purchaseReturnRepository.GetByIdAsync(item.PurchaseReturnId, cancellationToken);
            if (purchaseReturn is null || purchaseReturn.Status != PurchaseReturnStatus.DRAFT)
                return ServiceResult<bool>.Fail(ServiceErrorType.Validation, "Cannot modify a non-draft purchase return.");

            var deleted = await purchaseReturnRepository.DeleteItemAsync(command.PurchaseReturnItemId, cancellationToken);
            if (!deleted)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Purchase return item not found.");

            await purchaseReturnRepository.UpdateTotalAmountAsync(item.PurchaseReturnId, cancellationToken);

            logger.LogInformation("Deleted purchase return item {PurchaseReturnItemId}", command.PurchaseReturnItemId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting purchase return item {PurchaseReturnItemId}", command.PurchaseReturnItemId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}
