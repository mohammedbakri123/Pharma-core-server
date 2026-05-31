using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
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

            var updated = await purchaseRepository.UpdateItemAsync(item, cancellationToken);
            await purchaseRepository.UpdateTotalAmountAsync(command.PurchaseId, cancellationToken);

            logger.LogInformation("Purchase item {ItemId} updated", updated.PurchaseItemId);

            return ServiceResult<PurchaseItemDto>.Ok(
                new PurchaseItemDto(
                    updated.PurchaseItemId,
                    updated.MedicineId,
                    null,
                    updated.BatchId,
                    null,
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
