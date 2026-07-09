using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class GetMedicineMovementsService(
    IMedicineRepository medicineRepository,
    IStockMovementRepository stockMovementRepository,
    ILogger<GetMedicineMovementsService> logger)
    : IGetMedicineMovementsService
{
    public async Task<ServiceResult<PagedResult<StockMovementDto>>> ExecuteAsync(
        GetMedicineMovementsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var medicine = await medicineRepository.GetByIdAsync(query.MedicineId, cancellationToken);
            if (medicine is null)
                return ServiceResult<PagedResult<StockMovementDto>>.Fail(
                    ServiceErrorType.NotFound, "Medicine not found.");

            var page = query.Page <= 0 ? 1 : query.Page;
            var limit = query.Limit <= 0 ? 20 : query.Limit;

            var result = await stockMovementRepository.ListByMedicineIdAsync(
                query.MedicineId, page, limit, cancellationToken);

            var dtos = result.Items.Select(m => new StockMovementDto(
                m.StockMovementId,
                m.MedicineId,
                m.BatchId,
                m.Quantity,
                (int)m.Type,
                (int)m.ReferenceType,
                m.ReferenceId,
                m.CreatedAt ?? DateTime.UtcNow,
                m.MedicineName,
                m.BatchNumber)).ToList();

            return ServiceResult<PagedResult<StockMovementDto>>.Ok(
                new PagedResult<StockMovementDto>(dtos, result.Total, result.Page, result.Limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting movements for medicine {MedicineId}", query.MedicineId);
            return ServiceResult<PagedResult<StockMovementDto>>.Fail(
                ServiceErrorType.ServerError, e.Message);
        }
    }
}