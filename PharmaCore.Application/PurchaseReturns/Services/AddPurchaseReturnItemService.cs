using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class AddPurchaseReturnItemService(
    IPurchaseReturnRepository purchaseReturnRepository,
    IPurchaseReturnItemValidator validator,
    ILogger<AddPurchaseReturnItemService> logger)
    : IAddPurchaseReturnItemService
{
    public async Task<ServiceResult<PurchaseReturnItemDto>> ExecuteAsync(
        AddPurchaseReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await validator.ValidateAsync(
                command.PurchaseReturnId, command.PurchaseItemId, command.Quantity, cancellationToken: cancellationToken);

            if (!validation.IsValid)
                return ServiceResult<PurchaseReturnItemDto>.Fail(validation.ErrorType, validation.ErrorMessage!);

            var purchaseItem = validation.PurchaseItem!;

            var returnItem = PurchaseReturnItem.Create(
                command.PurchaseReturnId,
                command.PurchaseItemId,
                purchaseItem.BatchId ?? command.BatchId,
                command.Quantity,
                command.UnitPrice);

            var createdItem = await purchaseReturnRepository.AddItemAsync(returnItem, cancellationToken);
            await purchaseReturnRepository.UpdateTotalAmountAsync(command.PurchaseReturnId, cancellationToken);

            logger.LogInformation("Added item to purchase return {PurchaseReturnId}", command.PurchaseReturnId);

            return ServiceResult<PurchaseReturnItemDto>.Ok(new PurchaseReturnItemDto(
                createdItem.PurchaseReturnItemId,
                createdItem.PurchaseItemId,
                createdItem.BatchId,
                createdItem.Quantity,
                createdItem.UnitPrice,
                createdItem.TotalPrice));
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Invalid operation adding purchase return item");
            return ServiceResult<PurchaseReturnItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adding item to purchase return {PurchaseReturnId}", command.PurchaseReturnId);
            return ServiceResult<PurchaseReturnItemDto>.Fail(ServiceErrorType.ServerError, $"Error adding item: {e.Message}");
        }
    }
}
