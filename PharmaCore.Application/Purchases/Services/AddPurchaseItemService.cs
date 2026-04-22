using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class AddPurchaseItemService(IPurchaseRepository purchaseRepository, ILogger<AddPurchaseItemService> logger)
    : IAddPurchaseItemService
{
    public async Task<ServiceResult<PurchaseItemDto>> ExecuteAsync(AddPurchaseItemCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await purchaseRepository.GetByIdWithItemsAsync(command.PurchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {command.PurchaseId} not found.");
            }

            var item = PurchaseItem.Create(
                command.PurchaseId,
                command.MedicineId,
                command.BatchId,
                command.Quantity,
                command.PurchasePrice,
                command.SellPrice,
                command.ExpireDate);

            var created = await purchaseRepository.AddItemAsync(item, cancellationToken);
            await purchaseRepository.UpdateTotalAmountAsync(command.PurchaseId, cancellationToken);

            logger.LogInformation("Item {ItemId} added to purchase {PurchaseId}", created.PurchaseItemId, command.PurchaseId);

            return ServiceResult<PurchaseItemDto>.Ok(
                new PurchaseItemDto(
                    created.PurchaseItemId,
                    created.MedicineId,
                    null,
                    created.BatchId,
                    null,
                    created.Quantity,
                    created.PurchasePrice,
                    created.SellPrice,
                    created.TotalPrice,
                    created.ExpireDate));
        }
        catch (ArgumentException e)
        {
            return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adding item to purchase {PurchaseId}", command.PurchaseId);
            return ServiceResult<PurchaseItemDto>.Fail(ServiceErrorType.ServerError, $"Error adding item: {e.Message}");
        }
    }
}
