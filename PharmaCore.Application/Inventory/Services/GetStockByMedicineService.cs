using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class GetStockByMedicineService : IGetStockByMedicineService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<GetStockByMedicineService> _logger;

    public GetStockByMedicineService(
        IMedicineRepository medicineRepository,
        IBatchRepository batchRepository,
        ILogger<GetStockByMedicineService> logger)
    {
        _medicineRepository = medicineRepository;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<StockWithBatchesDto>> ExecuteAsync(GetStockByMedicineQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var medicine = await _medicineRepository.GetByIdAsync(query.MedicineId, cancellationToken);
            if (medicine is null)
                return ServiceResult<StockWithBatchesDto>.Fail(ServiceErrorType.NotFound, "Medicine not found.");

            var batches = await _batchRepository.ListAvailableByMedicineAsync(query.MedicineId, cancellationToken);

            var totalStock = batches.Sum(b => b.QuantityRemaining);

            var batchDtos = batches.Select(b => new BatchStockDto(
                b.BatchId,
                b.BatchNumber,
                b.QuantityRemaining,
                b.PurchasePrice,
                b.SellPrice,
                b.ExpireDate)).ToList();

            return ServiceResult<StockWithBatchesDto>.Ok(new StockWithBatchesDto(
                medicine.MedicineId,
                medicine.Name,
                totalStock,
                batchDtos));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting stock for medicine {MedicineId}", query.MedicineId);
            return ServiceResult<StockWithBatchesDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
