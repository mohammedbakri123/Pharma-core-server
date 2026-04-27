using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Application.Reports.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Services;

public class GetExpiredReportService(
    IBatchRepository batchRepository,
    IMedicineRepository medicineRepository,
    ILogger<GetExpiredReportService> logger)
    : IGetExpiredReportService
{
    public async Task<ServiceResult<ExpiredReportDto>> ExecuteAsync(DateTime? includeExpiredBefore, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = includeExpiredBefore.HasValue 
                ? DateOnly.FromDateTime(includeExpiredBefore.Value) 
                : DateOnly.FromDateTime(DateTime.UtcNow);

            var allBatches = await batchRepository.ListAvailableByMedicineAsync(0, cancellationToken);
            var expiredBatches = allBatches
                .Where(b => b.ExpireDate.HasValue && b.ExpireDate.Value < cutoffDate && b.QuantityRemaining > 0)
                .ToList();

            var medicines = await medicineRepository.ListAsync(cancellationToken);

            var expiredItems = expiredBatches.Select(b =>
            {
                var medicine = medicines.FirstOrDefault(m => m.MedicineId == b.MedicineId);
                return new ExpiredItemDto(
                    b.BatchId,
                    b.MedicineId,
                    medicine?.Name ?? "Unknown",
                    b.BatchNumber ?? "N/A",
                    b.QuantityRemaining,
                    b.ExpireDate!.Value,
                    b.QuantityRemaining * b.PurchasePrice);
            }).ToList();

            var totalExpiredValue = expiredItems.Sum(i => i.StockValue);

            return ServiceResult<ExpiredReportDto>.Ok(new ExpiredReportDto(
                expiredItems, totalExpiredValue));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating expired report");
            return ServiceResult<ExpiredReportDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
