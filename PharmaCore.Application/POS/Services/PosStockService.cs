using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Interfaces;
using PharmaCore.Application.POS.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.POS.Services;

public class PosStockService(
    IMedicineRepository medicineRepository,
    IBatchRepository batchRepository,
    ILogger<PosStockService> logger)
    : IPosStockService
{
    public async Task<ServiceResult<PosStockDto>> ExecuteAsync(PosStockQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var medicine = await medicineRepository.GetByIdAsync(query.MedicineId, cancellationToken);

            if (medicine is null || medicine.IsDeleted)
            {
                return ServiceResult<PosStockDto>.Fail(ServiceErrorType.NotFound, $"Medicine with ID {query.MedicineId} not found.");
            }

            var batches = await batchRepository.ListAvailableByMedicineAsync(query.MedicineId, cancellationToken);
            var totalStock = batches.Sum(b => b.QuantityRemaining);

            var batchDtos = batches
                .Where(b => b.QuantityRemaining > 0)
                .OrderBy(b => b.ExpireDate)
                .Select(b => new PosBatchDto(
                    b.BatchId,
                    b.BatchNumber,
                    b.QuantityRemaining,
                    b.SellPrice,
                    b.ExpireDate))
                .ToList();

            return ServiceResult<PosStockDto>.Ok(
                new PosStockDto(
                    medicine.MedicineId,
                    medicine.Name,
                    totalStock,
                    batchDtos));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting POS stock for medicine {MedicineId}", query.MedicineId);
            return ServiceResult<PosStockDto>.Fail(ServiceErrorType.ServerError, $"Error getting stock: {e.Message}");
        }
    }
}
