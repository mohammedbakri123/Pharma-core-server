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
            const int limit = 20;
            var result = await medicineRepository.ListAsync(
                1,
                limit,
                query.Q,
                null,
                null,
                cancellationToken);

            var results = new List<PosMedicineDto>();
            foreach (var medicine in result.Items)
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
