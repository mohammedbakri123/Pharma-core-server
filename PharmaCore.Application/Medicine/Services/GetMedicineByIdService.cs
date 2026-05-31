using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class GetMedicineByIdService : IGetMedicineByIdService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly ILogger<GetMedicineByIdService> _logger;

    public GetMedicineByIdService(IMedicineRepository medicineRepository, ILogger<GetMedicineByIdService> logger)
    {
        _medicineRepository = medicineRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<MedicineDto>> ExecuteAsync(GetMedicineByIdQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var medicine = await _medicineRepository.GetByIdAsync(query.MedicineId, cancellationToken);

            if (medicine == null)
            {
                return ServiceResult<MedicineDto>.Fail(ServiceErrorType.NotFound, "Medicine not found.");
            }

            return ServiceResult<MedicineDto>.Ok(MapToDto(medicine));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting medicine {MedicineId}", query.MedicineId);
            return ServiceResult<MedicineDto>.Fail(ServiceErrorType.ServerError, $"Error getting medicine: {e.Message}");
        }
    }

    private static MedicineDto MapToDto(PharmaCore.Domain.Entities.Medicine m) =>
        new MedicineDto(
            m.MedicineId,
            m.Name,
            m.ArabicName,
            m.Barcode,
            m.CategoryId,
            null,
            m.Unit,
            !m.IsDeleted,
            m.CreatedAt);
}
