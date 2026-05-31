using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class AddSalesReturnItemService : IAddSalesReturnItemService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ILogger<AddSalesReturnItemService> _logger;

    public AddSalesReturnItemService(
        ISalesReturnRepository salesReturnRepository,
        IBatchRepository batchRepository,
        IStockMovementRepository stockMovementRepository,
        ILogger<AddSalesReturnItemService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _batchRepository = batchRepository;
        _stockMovementRepository = stockMovementRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SalesReturnItemDto>> ExecuteAsync(AddSalesReturnItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await _salesReturnRepository.GetByIdAsync(command.SalesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            var unitPrice = command.UnitPrice ?? 0m;
            var returnItem = Domain.Entities.SalesReturnItem.Create(
                command.SalesReturnId,
                command.SaleItemId,
                command.BatchId,
                command.Quantity,
                unitPrice);

            var createdItem = await _salesReturnRepository.AddItemAsync(returnItem, cancellationToken);

            await _salesReturnRepository.UpdateTotalAmountAsync(command.SalesReturnId, cancellationToken);

            var stockMovement = Domain.Entities.StockMovement.Create(
                1,
                command.BatchId,
                command.Quantity,
                StockMovementType.IN,
                StockMovementReferenceType.RETURN,
                createdItem.SalesReturnId);

            await _stockMovementRepository.AddAsync(stockMovement, cancellationToken);

            await _batchRepository.IncrementBatchStockAsync(command.BatchId, command.Quantity, cancellationToken);

            _logger.LogInformation("Added item to sales return {SalesReturnId}, created stock movement", command.SalesReturnId);

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
            _logger.LogWarning(e, "Invalid operation adding sales return item");
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding item to sales return {SalesReturnId}", command.SalesReturnId);
            return ServiceResult<SalesReturnItemDto>.Fail(ServiceErrorType.ServerError, $"Error adding item: {e.Message}");
        }
    }
}