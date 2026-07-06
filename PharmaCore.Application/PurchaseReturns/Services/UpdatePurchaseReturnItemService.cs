using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class UpdatePurchaseReturnItemService(
    IPurchaseReturnRepository purchaseReturnRepository,
    IPurchaseReturnItemValidator validator,
    ILogger<UpdatePurchaseReturnItemService> logger)
    : IUpdatePurchaseReturnItemService
{
    public async Task<ServiceResult<PurchaseReturnItemDto>> ExecuteAsync(
        UpdatePurchaseReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await purchaseReturnRepository.GetItemByIdAsync(command.PurchaseReturnItemId, cancellationToken);
            if (item is null)
                return ServiceResult<PurchaseReturnItemDto>.Fail(ServiceErrorType.NotFound, "Purchase return item not found.");

            var validation = await validator.ValidateAsync(
                item.PurchaseReturnId, item.PurchaseItemId, command.Quantity, item.PurchaseReturnItemId, cancellationToken);

            if (!validation.IsValid)
                return ServiceResult<PurchaseReturnItemDto>.Fail(validation.ErrorType, validation.ErrorMessage!);

            item.UpdateQuantity(command.Quantity);

            var updated = await purchaseReturnRepository.UpdateItemAsync(item, cancellationToken);
            await purchaseReturnRepository.UpdateTotalAmountAsync(item.PurchaseReturnId, cancellationToken);

            logger.LogInformation("Updated purchase return item {PurchaseReturnItemId}", updated.PurchaseReturnItemId);

            return ServiceResult<PurchaseReturnItemDto>.Ok(new PurchaseReturnItemDto(
                updated.PurchaseReturnItemId,
                updated.PurchaseItemId,
                updated.BatchId,
                updated.Quantity,
                updated.UnitPrice,
                updated.TotalPrice));
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Invalid argument updating purchase return item");
            return ServiceResult<PurchaseReturnItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Invalid operation updating purchase return item");
            return ServiceResult<PurchaseReturnItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating purchase return item {PurchaseReturnItemId}", command.PurchaseReturnItemId);
            return ServiceResult<PurchaseReturnItemDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}
