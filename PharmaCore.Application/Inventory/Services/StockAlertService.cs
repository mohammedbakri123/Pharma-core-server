using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class StockAlertService(
    IBatchRepository batchRepository,
    ILogger<StockAlertService> logger) : IStockAlertService
{
    public async Task<ServiceResult<IReadOnlyList<StockAlertDto>>> ExecuteAsync(
        GetStockAlertQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = await batchRepository.GetStockAlertsAsync(
                query.LowStockThreshold, query.ExpiringDays, cancellationToken);

            return ServiceResult<IReadOnlyList<StockAlertDto>>.Ok(
                alerts.OrderBy(a => a.Status).ThenBy(a => a.TotalQuantity).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting stock alerts");
            return ServiceResult<IReadOnlyList<StockAlertDto>>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
