using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Application.Reports.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Services;

public class GetStockReportService(
    IMedicineRepository medicineRepository,
    IBatchRepository batchRepository,
    ICategoryRepository categoryRepository,
    ILogger<GetStockReportService> logger)
    : IGetStockReportService
{
    public async Task<ServiceResult<StockReportDto>> ExecuteAsync(int? categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var medicines = (await medicineRepository.ListAsync(cancellationToken)).ToList();
            var batches = (await batchRepository.ListAvailableByMedicineAsync(0, cancellationToken)).ToList();
            var categories = (await categoryRepository.ListAsync(cancellationToken)).ToList();

            if (categoryId.HasValue)
            {
                medicines = medicines.Where(m => m.CategoryId == categoryId).ToList();
            }

            var totalMedicines = medicines.Count;
            var totalBatches = batches.Count;
            var totalStockValue = batches.Sum(b => b.QuantityRemaining * b.PurchasePrice);

            var stockByCategory = categories.Select(c =>
            {
                var categoryMedicines = medicines.Where(m => m.CategoryId == c.CategoryId).Select(m => m.MedicineId).ToHashSet();
                var categoryBatches = batches.Where(b =>
                {
                    var medicine = medicines.FirstOrDefault(m => m.MedicineId == b.MedicineId);
                    return medicine != null && categoryMedicines.Contains(medicine.MedicineId);
                }).ToList();
                return new StockByCategoryDto(
                    c.CategoryId,
                    c.Name,
                    categoryBatches.Sum(b => b.QuantityRemaining),
                    categoryBatches.Sum(b => b.QuantityRemaining * b.PurchasePrice));
            }).ToList();

            var lowStockCount = 10; // TODO: get from config or use default
            var expiredCount = batches.Count(b => b.ExpireDate < DateOnly.FromDateTime(DateTime.UtcNow));
            var expiringSoonCount = batches.Count(b =>
                b.ExpireDate >= DateOnly.FromDateTime(DateTime.UtcNow) &&
                b.ExpireDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

            return ServiceResult<StockReportDto>.Ok(new StockReportDto(
                totalMedicines, totalBatches, totalStockValue,
                stockByCategory, lowStockCount, expiredCount, expiringSoonCount));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating stock report");
            return ServiceResult<StockReportDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
