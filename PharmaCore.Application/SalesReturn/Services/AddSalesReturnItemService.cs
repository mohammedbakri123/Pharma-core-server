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
    ILogger<AddSalesReturnItemService> logger)
    : IAddSalesReturnItemService
{
    public async Task<ServiceResult<SalesReturnItemDto>> ExecuteAsync(AddSalesReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await salesReturnRepository.GetByIdAsync(command.SalesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            if (salesReturn.Status != SalesReturnStatus.DRAFT)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, "Cannot modify a non-draft sales return.");

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
