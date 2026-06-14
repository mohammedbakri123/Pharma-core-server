using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class StockAlertService(
    IMedicineRepository medicineRepository,
    IBatchRepository batchRepository,
    ILogger<StockAlertService> logger) : IStockAlertService
{
    public async Task<ServiceResult<IReadOnlyList<StockAlertDto>>> ExecuteAsync(
        GetStockAlertQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var threshold = query.LowStockThreshold > 0 ? query.LowStockThreshold : 10;
            var days = query.ExpiringDays > 0 ? query.ExpiringDays : 30;
            var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var medicinePage = await medicineRepository.ListAsync(1, int.MaxValue, null, null, null, cancellationToken);
            var medicines = medicinePage.Items;

            var alerts = new List<StockAlertDto>();
            foreach (var med in medicines)
            {
                var batches = await batchRepository.ListAvailableByMedicineAsync(med.MedicineId, cancellationToken);
                var totalStock = batches.Sum(b => b.QuantityRemaining);

                var hasExpiringBatch = batches.Any(b =>
                    b.ExpireDate.HasValue && b.ExpireDate.Value <= cutoffDate && b.QuantityRemaining > 0);

                if (totalStock > threshold && !hasExpiringBatch)
                    continue;

                string status;
                if (totalStock <= 5)
                    status = "حرج";
                else if (totalStock <= threshold)
                    status = "مخزون منخفض";
                else
                    status = "متوفر";

                alerts.Add(new StockAlertDto(
                    med.MedicineId,
                    med.Name,
                    med.ArabicName,
                    med.Barcode,
                    med.CategoryName,
                    (int?)med.Unit,
                    totalStock,
                    status));
            }

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
