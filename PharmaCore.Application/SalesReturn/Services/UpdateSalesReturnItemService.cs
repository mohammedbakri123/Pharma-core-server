using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class UpdateSalesReturnItemService : IUpdateSalesReturnItemService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<UpdateSalesReturnItemService> _logger;

    public UpdateSalesReturnItemService(
        ISalesReturnRepository salesReturnRepository,
        IStockMovementRepository stockMovementRepository,
        IBatchRepository batchRepository,
        ILogger<UpdateSalesReturnItemService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _stockMovementRepository = stockMovementRepository;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SalesReturnItemDto>> ExecuteAsync(UpdateSalesReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _salesReturnRepository.GetItemByIdAsync(command.SalesReturnItemId, cancellationToken);
            if (item is null)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.NotFound, "Sales return item not found.");

            var oldQuantity = item.Quantity;
            var quantityDiff = command.Quantity - oldQuantity;

            item.UpdateQuantity(command.Quantity);

            var updated = await _salesReturnRepository.UpdateItemAsync(item, cancellationToken);
            await _salesReturnRepository.UpdateTotalAmountAsync(item.SalesReturnId, cancellationToken);

            if (quantityDiff != 0)
            {
                if (quantityDiff > 0)
                {
                    await _batchRepository.IncrementBatchStockAsync(item.BatchId, quantityDiff, cancellationToken);
                }
                else
                {
                    await _batchRepository.DecrementBatchStockAsync(item.BatchId, Math.Abs(quantityDiff), cancellationToken);
                }
            }

            _logger.LogInformation("Updated sales return item {SalesReturnItemId}", updated.SalesReturnItemId);

            return ServiceResult<SalesReturnItemDto>.Ok(new SalesReturnItemDto(
                updated.SalesReturnItemId,
                updated.SalesReturnId,
                updated.SaleItemId,
                updated.BatchId,
                updated.Quantity,
                updated.UnitPrice,
                updated.TotalPrice));
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning(e, "Invalid operation updating sales return item");
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating sales return item {SalesReturnItemId}", command.SalesReturnItemId);
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}