using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class UpdateSalesReturnItemService(
    ISalesReturnRepository salesReturnRepository,
    ISalesReturnItemValidator validator,
    ILogger<UpdateSalesReturnItemService> logger)
    : IUpdateSalesReturnItemService
{
    public async Task<ServiceResult<SalesReturnItemDto>> ExecuteAsync(UpdateSalesReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await salesReturnRepository.GetItemByIdAsync(command.SalesReturnItemId, cancellationToken);
            if (item is null)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.NotFound, "Sales return item not found.");

            var validation = await validator.ValidateAsync(
                item.SalesReturnId, item.SaleItemId, command.Quantity, item.SalesReturnItemId, cancellationToken);

            if (!validation.IsValid)
                return ServiceResult<SalesReturnItemDto>.Fail(validation.ErrorType, validation.ErrorMessage!);

            item.UpdateQuantity(command.Quantity);

            var updated = await salesReturnRepository.UpdateItemAsync(item, cancellationToken);
            await salesReturnRepository.UpdateTotalAmountAsync(item.SalesReturnId, cancellationToken);

            logger.LogInformation("Updated sales return item {SalesReturnItemId}", updated.SalesReturnItemId);

            return ServiceResult<SalesReturnItemDto>.Ok(new SalesReturnItemDto(
                updated.SalesReturnItemId,
                updated.SalesReturnId,
                updated.SaleItemId,
                updated.BatchId,
                updated.Quantity,
                updated.UnitPrice,
                updated.TotalPrice));
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Invalid argument updating sales return item");
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Invalid operation updating sales return item");
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating sales return item {SalesReturnItemId}", command.SalesReturnItemId);
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}