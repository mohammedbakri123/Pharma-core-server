using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class GetExpiringService : IGetExpiringService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly ILogger<GetExpiringService> _logger;

    public GetExpiringService(
        IBatchRepository batchRepository,
        IMedicineRepository medicineRepository,
        ILogger<GetExpiringService> logger)
    {
        _batchRepository = batchRepository;
        _medicineRepository = medicineRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<ExpiringItemDto>>> ExecuteAsync(GetExpiringQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var days = query.DaysUntilExpiry > 0 ? query.DaysUntilExpiry : 30;
            var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));

            var medicines = await _medicineRepository.ListAsync(cancellationToken);
            var expiringItems = new List<ExpiringItemDto>();

            foreach (var med in medicines)
            {
                var batches = await _batchRepository.ListAvailableByMedicineAsync(med.MedicineId, cancellationToken);
                foreach (var batch in batches)
                {
                    if (batch.ExpireDate.HasValue && batch.ExpireDate.Value <= cutoffDate && batch.QuantityRemaining > 0)
                    {
                        var daysLeft = batch.ExpireDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
                        expiringItems.Add(new ExpiringItemDto(
                            batch.BatchId,
                            med.MedicineId,
                            med.Name,
                            batch.BatchNumber,
                            batch.QuantityRemaining,
                            batch.ExpireDate.Value,
                            daysLeft));
                    }
                }
            }

            return ServiceResult<IReadOnlyList<ExpiringItemDto>>.Ok(
                expiringItems.OrderBy(e => e.DaysUntilExpiry).ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting expiring items");
            return ServiceResult<IReadOnlyList<ExpiringItemDto>>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
