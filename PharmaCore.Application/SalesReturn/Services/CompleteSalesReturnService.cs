using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class CompleteSalesReturnService(
    ISalesReturnRepository salesReturnRepository,
    ISaleRepository saleRepository,
    IBatchRepository batchRepository,
    IStockMovementRepository stockMovementRepository,
    ILogger<CompleteSalesReturnService> logger)
    : ICompleteSalesReturnService
{
    public async Task<ServiceResult<CompleteSalesReturnResultDto>> ExecuteAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await salesReturnRepository.GetByIdWithItemsAsync(salesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<CompleteSalesReturnResultDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            if (salesReturn.Status != SalesReturnStatus.Draft)
                return ServiceResult<CompleteSalesReturnResultDto>.Fail(ServiceErrorType.Validation, "Only draft sales returns can be completed.");

            if (salesReturn.Items.Count == 0)
                return ServiceResult<CompleteSalesReturnResultDto>.Fail(ServiceErrorType.Validation, "Cannot complete a sales return without items.");

            var stockMovements = new List<StockMovement>();

            foreach (var item in salesReturn.Items)
            {
                var saleItem = await saleRepository.GetItemByIdAsync(item.SaleItemId, cancellationToken);
                if (saleItem is null)
                    return ServiceResult<CompleteSalesReturnResultDto>.Fail(ServiceErrorType.NotFound, $"Sale item {item.SaleItemId} not found.");

                await batchRepository.IncrementBatchStockAsync(item.BatchId, item.Quantity, cancellationToken);

                stockMovements.Add(StockMovement.Create(
                    saleItem.MedicineId,
                    item.BatchId,
                    item.Quantity,
                    StockMovementType.IN,
                    StockMovementReferenceType.RETURN,
                    salesReturn.SalesReturnId));
            }

            await stockMovementRepository.AddRangeAsync(stockMovements, cancellationToken);

            salesReturn.Complete();
            var updated = await salesReturnRepository.UpdateAsync(salesReturn, cancellationToken);

            var result = new CompleteSalesReturnResultDto(
                updated.SalesReturnId,
                updated.Status,
                updated.TotalAmount,
                DateTime.UtcNow,
                stockMovements.Count);

            return ServiceResult<CompleteSalesReturnResultDto>.Ok(result);
        }
        catch (ArgumentException e)
        {
            return ServiceResult<CompleteSalesReturnResultDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (InvalidOperationException e)
        {
            return ServiceResult<CompleteSalesReturnResultDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error completing sales return {SalesReturnId}", salesReturnId);
            return ServiceResult<CompleteSalesReturnResultDto>.Fail(ServiceErrorType.ServerError, $"Error completing sales return: {e.Message}");
        }
    }
}
