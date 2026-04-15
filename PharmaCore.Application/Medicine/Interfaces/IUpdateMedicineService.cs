using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Interfaces;

public interface IUpdateMedicineService
{
    Task<ServiceResult<MedicineDto>> ExecuteAsync(UpdateMedicineCommand command, CancellationToken cancellationToken = default);
}
