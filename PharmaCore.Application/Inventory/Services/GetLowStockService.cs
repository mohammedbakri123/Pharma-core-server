using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class GetLowStockService : IGetLowStockService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<GetLowStockService> _logger;

    public GetLowStockService(
        IMedicineRepository medicineRepository,
        IBatchRepository batchRepository,
        ILogger<GetLowStockService> logger)
    {
        _medicineRepository = medicineRepository;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<LowStockItemDto>>> ExecuteAsync(GetLowStockQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var thresholdValue = query.Threshold > 0 ? query.Threshold : 10;
            var medicines = await _medicineRepository.ListAsync(cancellationToken);

            var lowStockItems = new List<LowStockItemDto>();
            foreach (var med in medicines)
            {
                var batches = await _batchRepository.ListAvailableByMedicineAsync(med.MedicineId, cancellationToken);
                var totalStock = batches.Sum(b => b.QuantityRemaining);

                if (totalStock <= thresholdValue)
                {
                    lowStockItems.Add(new LowStockItemDto(
                        med.MedicineId,
                        med.Name,
                        med.Barcode,
                        totalStock));
                }
            }

            return ServiceResult<IReadOnlyList<LowStockItemDto>>.Ok(lowStockItems.OrderBy(l => l.CurrentStock).ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting low stock");
            return ServiceResult<IReadOnlyList<LowStockItemDto>>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
