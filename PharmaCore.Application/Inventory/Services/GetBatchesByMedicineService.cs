using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class GetBatchesByMedicineService : IGetBatchesByMedicineService
{
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<GetBatchesByMedicineService> _logger;

    public GetBatchesByMedicineService(
        IBatchRepository batchRepository,
        ILogger<GetBatchesByMedicineService> logger)
    {
        _batchRepository = batchRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<BatchStockDto>>> ExecuteAsync(GetBatchesByMedicineQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var batches = await _batchRepository.ListAvailableByMedicineAsync(query.MedicineId, cancellationToken);

            var batchDtos = batches.Select(b => new BatchStockDto(
                b.BatchId,
                b.BatchNumber,
                b.QuantityRemaining,
                b.PurchasePrice,
                b.SellPrice,
                b.ExpireDate)).ToList();

            return ServiceResult<IReadOnlyList<BatchStockDto>>.Ok(batchDtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting batches for medicine {MedicineId}", query.MedicineId);
            return ServiceResult<IReadOnlyList<BatchStockDto>>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
