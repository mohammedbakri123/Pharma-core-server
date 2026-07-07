using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class AddSalesReturnItemService(
    ISalesReturnRepository salesReturnRepository,
    ISalesReturnItemValidator validator,
    ILogger<AddSalesReturnItemService> logger)
    : IAddSalesReturnItemService
{
    public async Task<ServiceResult<SalesReturnItemDto>> ExecuteAsync(AddSalesReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = await validator.ValidateAsync(
                command.SalesReturnId, command.SaleItemId, command.Quantity, cancellationToken: cancellationToken);

            if (!validation.IsValid)
                return ServiceResult<SalesReturnItemDto>.Fail(validation.ErrorType, validation.ErrorMessage!);

            var saleItem = validation.SaleItem!;

            var unitPrice = command.UnitPrice ?? saleItem.UnitPrice;
            var returnItem = SalesReturnItem.Create(
                command.SalesReturnId,
                command.SaleItemId,
                command.BatchId,
                null,
                null,
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
