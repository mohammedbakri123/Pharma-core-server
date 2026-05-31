using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Interfaces;
using PharmaCore.Application.POS.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.POS.Services;

public class PosScanService(
    IMedicineRepository medicineRepository,
    IBatchRepository batchRepository,
    ILogger<PosScanService> logger)
    : IPosScanService
{
    public async Task<ServiceResult<PosMedicineDto>> ExecuteAsync(PosScanQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var medicines = await medicineRepository.ListAsync(cancellationToken);
            var medicine = medicines
                .Where(m => !m.IsDeleted)
                .FirstOrDefault(m => m.Barcode != null && m.Barcode.Equals(query.Barcode, StringComparison.OrdinalIgnoreCase));

            if (medicine is null)
            {
                return ServiceResult<PosMedicineDto>.Fail(ServiceErrorType.NotFound, $"Medicine with barcode '{query.Barcode}' not found.");
            }

            var batches = await batchRepository.ListAvailableByMedicineAsync(medicine.MedicineId, cancellationToken);
            var totalStock = batches.Sum(b => b.QuantityRemaining);
            var sellPrice = batches.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.SellPrice ?? 0m;

            return ServiceResult<PosMedicineDto>.Ok(
                new PosMedicineDto(
                    medicine.MedicineId,
                    medicine.Name,
                    medicine.ArabicName,
                    medicine.Barcode,
                    medicine.Unit?.ToString(),
                    sellPrice,
                    totalStock));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error scanning barcode '{Barcode}'", query.Barcode);
            return ServiceResult<PosMedicineDto>.Fail(ServiceErrorType.ServerError, $"Error scanning: {e.Message}");
        }
    }
}
