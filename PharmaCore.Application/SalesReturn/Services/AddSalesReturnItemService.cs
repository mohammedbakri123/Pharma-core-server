using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class AddSalesReturnItemService(
    ISalesReturnRepository salesReturnRepository,
    ISaleRepository salesRepository,
    ILogger<AddSalesReturnItemService> logger)
    : IAddSalesReturnItemService
{
    public async Task<ServiceResult<SalesReturnItemDto>> ExecuteAsync(AddSalesReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await salesReturnRepository.GetByIdWithItemsAsync(command.SalesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            if (salesReturn.Status != SalesReturnStatus.Draft)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, "Cannot modify a non-draft sales return.");

            if (command.Quantity <= 0)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, "Quantity must be greater than zero.");

            var saleItem = await salesRepository.GetItemByIdAsync(command.SaleItemId, cancellationToken);
            if (saleItem is null)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, "Sale item not found.");

            var currentDraftQuantity = salesReturn.Items
                .Where(i => i.SaleItemId == command.SaleItemId)
                .Sum(i => i.Quantity);

            var completedReturnQuantity = await salesReturnRepository
                .GetCompletedReturnQuantityBySaleItemAsync(command.SaleItemId, cancellationToken);

            var totalReturned = currentDraftQuantity + completedReturnQuantity + command.Quantity;

            logger.LogInformation(
                "SaleItem {SaleItemId}: original={OriginalQty}, draft={DraftQty}, completed={CompletedQty}, requested={RequestedQty}",
                command.SaleItemId, saleItem.Quantity, currentDraftQuantity, completedReturnQuantity, command.Quantity);

            if (totalReturned > saleItem.Quantity)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation,
                    $"Return quantity exceeds original sale quantity ({saleItem.Quantity}).");

            var unitPrice = command.UnitPrice ?? saleItem.UnitPrice;
            var returnItem = SalesReturnItem.Create(
                command.SalesReturnId,
                command.SaleItemId,
                command.BatchId,
                command.Quantity,
                unitPrice);

            var createdItem = await salesReturnRepository.AddItemAsync(returnItem, cancellationToken);

            await salesReturnRepository.UpdateTotalAmountAsync(command.SalesReturnId, cancellationToken);

            logger.LogInformation("Added item to sales return {SalesReturnId}", command.SalesReturnId);

            return ServiceResult<SalesReturnItemDto>.Ok(new SalesReturnItemDto(
                createdItem.SalesReturnItemId,
                createdItem.SalesReturnId,
                createdItem.SaleItemId,
                createdItem.BatchId,
                createdItem.Quantity,
                createdItem.UnitPrice,
                createdItem.TotalPrice));
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Invalid operation adding sales return item");
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adding item to sales return {SalesReturnId}", command.SalesReturnId);
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.ServerError, $"Error adding item: {e.Message}");
        }
    }
}
