using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
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
        GetStockQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<StockItemDto>>.Fail(ServiceErrorType.Validation, "Invalid pagination.");

            var medicines = await medicineRepository.ListAsync(cancellationToken);
            var filtered = medicines.ToList();

            if (query.MedicineId.HasValue)
                filtered = filtered.Where(m => m.MedicineId == query.MedicineId.Value).ToList();

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
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToList();

            return ServiceResult<PagedResult<StockItemDto>>.Ok(
                new PagedResult<StockItemDto>(items, total, query.Page, query.Limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting stock");
            return ServiceResult<PagedResult<StockItemDto>>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
