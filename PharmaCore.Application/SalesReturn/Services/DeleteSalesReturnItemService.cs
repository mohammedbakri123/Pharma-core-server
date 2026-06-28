using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class DeleteSalesReturnItemService : IDeleteSalesReturnItemService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly ILogger<DeleteSalesReturnItemService> _logger;

    public DeleteSalesReturnItemService(
        ISalesReturnRepository salesReturnRepository,
        ILogger<DeleteSalesReturnItemService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(DeleteSalesReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _salesReturnRepository.GetItemByIdAsync(command.SalesReturnItemId, cancellationToken);
            if (item is null)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Sales return item not found.");

            var salesReturn = await _salesReturnRepository.GetByIdAsync(item.SalesReturnId, cancellationToken);
            if (salesReturn is null || salesReturn.Status != SalesReturnStatus.Draft)
                return ServiceResult<bool>.Fail(ServiceErrorType.Validation, "Cannot modify a non-draft sales return.");

            var deleted = await _salesReturnRepository.DeleteItemAsync(command.SalesReturnItemId, cancellationToken);
            if (!deleted)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Sales return item not found.");

            await _salesReturnRepository.UpdateTotalAmountAsync(item.SalesReturnId, cancellationToken);

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
