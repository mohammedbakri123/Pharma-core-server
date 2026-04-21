using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class RestoreMedicineService(IMedicineRepository medicineRepository, ILogger<RestoreMedicineService> logger)
    : IRestoreMedicineService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(RestoreMedicineCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await medicineRepository.RestoreDeletedAsync(command.MedicineId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Deleted medicine with ID {command.MedicineId} not found or is not deleted.");
            }

            logger.LogInformation("Medicine with ID {MedicineId} restored successfully", command.MedicineId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error restoring medicine");
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error restoring medicine: {e.Message}");
        }
    }
}
