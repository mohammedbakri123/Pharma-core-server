using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class UpdateSaleItemService : IUpdateSaleItemService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<UpdateSaleItemService> _logger;

    public UpdateSaleItemService(ISaleRepository saleRepository, IBatchRepository batchRepository, ILogger<UpdateSaleItemService> logger)
    {
        _saleRepository = saleRepository;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SaleItemDto>> ExecuteAsync(UpdateSaleItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
            if (sale is null || sale.Status != SaleStatus.DRAFT)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Sale not found or not a draft.");

            var item = await _saleRepository.GetItemByIdAsync(command.ItemId, cancellationToken);
            if (item is null || item.SaleId != command.SaleId)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Item not found.");

            var batch = await _batchRepository.GetByIdAsync(item.BatchId, cancellationToken);
            if (batch is null)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Batch not found.");

            if (batch.QuantityRemaining < command.Quantity)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.Validation, "Insufficient stock.");

            item.UpdateQuantity(command.Quantity);
            var updated = await _saleRepository.UpdateItemAsync(item, cancellationToken);
            await _saleRepository.UpdateTotalAmountAsync(command.SaleId, cancellationToken);

            return ServiceResult<SaleItemDto>.Ok(SaleMappings.MapItem(updated));
        }
        catch (ArgumentException e)
        {
            return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating sale item {ItemId}", command.ItemId);
            return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.ServerError, $"Error updating sale item: {e.Message}");
        }
    }
}