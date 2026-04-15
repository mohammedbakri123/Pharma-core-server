using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class GetMedicineByIdService : IGetMedicineByIdService
{
    private readonly IMedicineRepository _medicineRepository;

    public GetMedicineByIdService(IMedicineRepository medicineRepository)
    {
        _medicineRepository = medicineRepository;
    }

    public async Task<ServiceResult<MedicineDto>> ExecuteAsync(GetMedicineByIdQuery query, CancellationToken cancellationToken = default)
    {
        var medicine = await _medicineRepository.GetByIdAsync(query.MedicineId, cancellationToken);

        if (medicine == null)
        {
            return ServiceResult<MedicineDto>.Fail(ServiceErrorType.NotFound, "Medicine not found.");
        }

        return ServiceResult<MedicineDto>.Ok(MapToDto(medicine));
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
            !m.IsDeleted.GetValueOrDefault(false),
            m.CreatedAt);
}
