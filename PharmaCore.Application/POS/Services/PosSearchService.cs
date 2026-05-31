using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Interfaces;
using PharmaCore.Application.POS.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.POS.Services;

public class PosSearchService(
    IMedicineRepository medicineRepository,
    IBatchRepository batchRepository,
    ILogger<PosSearchService> logger)
    : IPosSearchService
{
    public async Task<ServiceResult<List<PosMedicineDto>>> ExecuteAsync(PosSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var medicines = await medicineRepository.ListAsync(cancellationToken);
            var search = query.Q.ToLower();

            var filtered = medicines
                .Where(m => !m.IsDeleted)
                .Where(m =>
                    m.Name.Contains(query.Q, StringComparison.OrdinalIgnoreCase) ||
                    (m.ArabicName != null && m.ArabicName.Contains(query.Q, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Barcode != null && m.Barcode.Contains(query.Q, StringComparison.OrdinalIgnoreCase)))
                .Take(20)
                .ToList();

            var results = new List<PosMedicineDto>();
            foreach (var medicine in filtered)
            {
                var batches = await batchRepository.ListAvailableByMedicineAsync(medicine.MedicineId, cancellationToken);
                var totalStock = batches.Sum(b => b.QuantityRemaining);
                var sellPrice = batches.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.SellPrice ?? 0m;

                results.Add(new PosMedicineDto(
                    medicine.MedicineId,
                    medicine.Name,
                    medicine.ArabicName,
                    medicine.Barcode,
                    medicine.Unit?.ToString(),
                    sellPrice,
                    totalStock));
            }

            return ServiceResult<List<PosMedicineDto>>.Ok(results);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in POS search for '{Q}'", query.Q);
            return ServiceResult<List<PosMedicineDto>>.Fail(ServiceErrorType.ServerError, $"Error searching: {e.Message}");
        }
    }
}
