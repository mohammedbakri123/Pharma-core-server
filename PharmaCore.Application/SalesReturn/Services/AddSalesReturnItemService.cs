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
            //1 - check sales return exist
            var salesReturn = await salesReturnRepository.GetByIdWithItemsAsync(command.SalesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");
            
            
            //2 - check it is draft
            if (salesReturn.Status != SalesReturnStatus.Draft)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, "Cannot modify a non-draft sales return.");

            
            
            
            //3 - check that there is enough quantity
            //A - get sales item total quantity
            var saleItem = await salesRepository.GetItemByIdAsync(command.SaleItemId, cancellationToken);
            
            if (saleItem is null)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, "Cannot returns item does not exist in sales item list.");

            int itemTotalQuantity = saleItem.Quantity;
            
            //B - get current sale return draft 
            var currentDraftQuantity = salesReturn.Items.Where(i => i.SaleItemId == command.SaleItemId).Sum(i => i.Quantity);
            
            //C - get all completed returns created for that sale item

            var completedReturns = await salesReturnRepository.GetBySaleIdWithItemsAsync(salesReturn.SaleId,SalesReturnStatus.Completed, cancellationToken);
            logger.LogInformation("Completed returns count: {Count}", completedReturns.Count());
            int alreadyReturnedItemsQuantity = 0;
            foreach (var r in completedReturns)
            {
                alreadyReturnedItemsQuantity += r.Items
                    .Where(i => i.SaleItemId == command.SaleItemId)
                    .Sum(i => i.Quantity);
            }
            
            logger.LogInformation("Item total quantity: {Quantity}", itemTotalQuantity);
            logger.LogInformation("Current draft quantity: {Quantity}", currentDraftQuantity);
            logger.LogInformation("Already returned quantity: {Quantity}", alreadyReturnedItemsQuantity);
            logger.LogInformation("Requested quantity: {Quantity}", command.Quantity);
            logger.LogInformation(
                "Total after return: {Total}",
                alreadyReturnedItemsQuantity + currentDraftQuantity + command.Quantity);
            
            //D - Check 
            if (alreadyReturnedItemsQuantity + currentDraftQuantity + command.Quantity > itemTotalQuantity)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, "Cannot returns item, it is already returned.");

            
            
            
            var unitPrice = command.UnitPrice ?? 0m;
            var returnItem = Domain.Entities.SalesReturnItem.Create(
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
