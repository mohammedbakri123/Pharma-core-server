using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class UpdatePurchaseItemService(IPurchaseRepository purchaseRepository, ILogger<UpdatePurchaseItemService> logger)
    : IUpdatePurchaseItemService
{
    public async Task<ServiceResult<PurchaseItemDto>> ExecuteAsync(UpdatePurchaseItemCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await purchaseRepository.GetItemByIdAsync(command.ItemId, cancellationToken);

            if (item is null)
            {
                return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.NotFound, $"Purchase item with ID {command.ItemId} not found.");
            }

            if (item.PurchaseId != command.PurchaseId)
            {
                return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.Validation, "Purchase item does not belong to the specified purchase.");
            }

            var purchase = await purchaseRepository.GetByIdAsync(command.PurchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {command.PurchaseId} not found.");
            }

            if (purchase.Status != PurchaseStatus.DRAFT)
            {
                return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.Validation, "Only draft purchases can be modified.");
            }

            if (command.Quantity.HasValue)
            {
                item.UpdateQuantity(command.Quantity.Value);
            }

            if (command.PurchasePrice.HasValue || command.SellPrice.HasValue)
            {
                item.UpdatePrices(
                    command.PurchasePrice ?? item.PurchasePrice,
                    command.SellPrice ?? item.SellPrice);
            }

            if (command.BatchNumber is not null)
            {
                item.UpdateBatchNumber(command.BatchNumber);
            }

            var updated = await purchaseRepository.UpdateItemAsync(item, cancellationToken);
            await purchaseRepository.UpdateTotalAmountAsync(command.PurchaseId, cancellationToken);

            logger.LogInformation("Purchase item {ItemId} updated", updated.PurchaseItemId);

            return ServiceResult<PurchaseItemDto>.Ok(
                new PurchaseItemDto(
                    updated.PurchaseItemId,
                    updated.MedicineId,
                    null,
                    updated.BatchId,
                    updated.BatchNumber,
                    updated.Quantity,
                    updated.PurchasePrice,
                    updated.SellPrice,
                    updated.TotalPrice,
                    updated.ExpireDate));
        }
        catch (ArgumentException e)
        {
            return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating purchase item {ItemId}", command.ItemId);
            return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.ServerError, $"Error updating item: {e.Message}");
        }
    }
}
