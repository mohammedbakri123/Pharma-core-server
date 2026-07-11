using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class StockAlertService(
    IBatchRepository batchRepository,
    ILogger<StockAlertService> logger) : IStockAlertService
{
    public async Task<ServiceResult<PagedResult<StockAlertDto>>> ExecuteAsync(
        GetStockAlertQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<StockAlertDto>>.Fail(ServiceErrorType.Validation, "Invalid pagination.");

            var result = await batchRepository.GetStockAlertsAsync(
                query.LowStockThreshold, query.ExpiringDays, query.SearchTerm, query.Page, query.Limit, query.ExcludeZeroStock, cancellationToken);

            return ServiceResult<PagedResult<StockAlertDto>>.Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting stock alerts");
            return ServiceResult<PagedResult<StockAlertDto>>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
