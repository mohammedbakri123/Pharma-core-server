using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class GetStockService(
    IMedicineRepository medicineRepository,
    IBatchRepository batchRepository,
    ILogger<GetStockService> logger)
    : IGetStockService
{
    public async Task<ServiceResult<PagedResult<StockItemDto>>> ExecuteAsync(
        int page,
        int limit,
        int? medicineId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page <= 0 || limit <= 0)
                return ServiceResult<PagedResult<StockItemDto>>.Fail(ServiceErrorType.Validation, "Invalid pagination.");

            var medicines = await medicineRepository.ListAsync(cancellationToken);
            var filtered = medicines.ToList();

            if (medicineId.HasValue)
                filtered = filtered.Where(m => m.MedicineId == medicineId.Value).ToList();

            var stockItems = new List<StockItemDto>();
            foreach (var med in filtered)
            {
                var batches = await batchRepository.ListAvailableByMedicineAsync(med.MedicineId, cancellationToken);
                var totalStock = batches.Sum(b => b.QuantityRemaining);
                stockItems.Add(new StockItemDto(
                    med.MedicineId,
                    med.Name,
                    med.Barcode,
                    med.Unit?.ToString() ?? "Unknown",
                    totalStock));
            }

            var total = stockItems.Count;
            var items = stockItems
                .OrderBy(s => s.MedicineName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return ServiceResult<PagedResult<StockItemDto>>.Ok(
                new PagedResult<StockItemDto>(items, total, page, limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting stock");
            return ServiceResult<PagedResult<StockItemDto>>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}