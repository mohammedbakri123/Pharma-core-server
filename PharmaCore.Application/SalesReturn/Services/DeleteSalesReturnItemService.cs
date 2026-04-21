using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class DeleteSalesReturnItemService : IDeleteSalesReturnItemService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<DeleteSalesReturnItemService> _logger;

    public DeleteSalesReturnItemService(
        ISalesReturnRepository salesReturnRepository,
        IBatchRepository batchRepository,
        ILogger<DeleteSalesReturnItemService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(DeleteSalesReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _salesReturnRepository.GetItemByIdAsync(command.SalesReturnItemId, cancellationToken);
            if (item is null)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Sales return item not found.");

            var deleted = await _salesReturnRepository.DeleteItemAsync(command.SalesReturnItemId, cancellationToken);
            
            if (!deleted)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Sales return item not found.");

            await _salesReturnRepository.UpdateTotalAmountAsync(item.SalesReturnId, cancellationToken);

            await _batchRepository.DecrementBatchStockAsync(item.BatchId, item.Quantity, cancellationToken);

            _logger.LogInformation("Deleted sales return item {SalesReturnItemId}", command.SalesReturnItemId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting sales return item {SalesReturnItemId}", command.SalesReturnItemId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}