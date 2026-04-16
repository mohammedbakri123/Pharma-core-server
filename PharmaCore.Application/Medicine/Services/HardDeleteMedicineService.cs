using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class HardDeleteMedicineService : IHardDeleteMedicineService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly ILogger<HardDeleteMedicineService> _logger;

    public HardDeleteMedicineService(IMedicineRepository medicineRepository, ILogger<HardDeleteMedicineService> logger)
    {
        _medicineRepository = medicineRepository;
        _logger = logger;
    }
    public async Task<ServiceResult<bool>> ExecuteAsync(DeleteMedicineCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var medicine = await _medicineRepository.GetByIdAsync(command.MedicineId, cancellationToken);

            if (medicine == null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound,
                    $"Medicine with ID {command.MedicineId} not found.");
            }

            medicine.MarkDeleted();

            var result = await _medicineRepository.HardDeleteAsync(command.MedicineId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to delete medicine.");
            }

            _logger.LogInformation("Medicine with ID {Id} deleted successfully", command.MedicineId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            string errMessage = $"Failed to permanently delete medicine {e.Message} {e.StackTrace} {e.InnerException?.Message}";
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, errMessage);

        }
    }

}