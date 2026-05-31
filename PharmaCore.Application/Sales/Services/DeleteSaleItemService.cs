using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class DeleteSaleItemService : IDeleteSaleItemService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<DeleteSaleItemService> _logger;

    public DeleteSaleItemService(ISaleRepository saleRepository, ILogger<DeleteSaleItemService> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(DeleteSaleItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
            if (sale is null || sale.Status != SaleStatus.DRAFT)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Sale not found or not a draft.");

            var item = await _saleRepository.GetItemByIdAsync(command.ItemId, cancellationToken);
            if (item is null || item.SaleId != command.SaleId)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Item not found.");

            var deleted = await _saleRepository.DeleteItemAsync(command.ItemId, cancellationToken);
            if (!deleted)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Item not found.");

            await _saleRepository.UpdateTotalAmountAsync(command.SaleId, cancellationToken);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting sale item {ItemId}", command.ItemId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error deleting sale item: {e.Message}");
        }
    }
}