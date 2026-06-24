using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class UpdateSaleItemService(
    ISaleRepository saleRepository,
    IBatchRepository batchRepository,
    ILogger<UpdateSaleItemService> logger)
    : IUpdateSaleItemService
{
    public async Task<ServiceResult<SaleItemDto>> ExecuteAsync(UpdateSaleItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await saleRepository.GetByIdWithItemsAsync(command.SaleId, cancellationToken);
            if (sale is null || sale.Status != SaleStatus.DRAFT)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Sale not found or not a draft.");

            var item = sale.Items.FirstOrDefault(i => i.SaleItemId == command.ItemId);
            if (item is null)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Item not found.");

            var batch = await batchRepository.GetByIdAsync(item.BatchId, cancellationToken);
            if (batch is null)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Batch not found.");

            var otherReserved = sale.Items
                .Where(i => i.BatchId == item.BatchId && i.SaleItemId != command.ItemId)
                .Sum(i => i.Quantity);

            var available = batch.QuantityRemaining - otherReserved;
            if (available < command.Quantity)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.Validation, "Insufficient stock.");

            item.UpdateQuantity(command.Quantity);
            var updated = await saleRepository.UpdateItemAsync(item, cancellationToken);
            await saleRepository.UpdateTotalAmountAsync(command.SaleId, cancellationToken);

            return ServiceResult<SaleItemDto>.Ok(SaleMappings.MapItem(updated));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating sale item {ItemId}", command.ItemId);
            return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.ServerError, $"Error updating sale item: {e.Message}");
        }
    }
}