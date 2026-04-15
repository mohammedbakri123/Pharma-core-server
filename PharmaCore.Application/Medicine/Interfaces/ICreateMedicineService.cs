using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Interfaces;

public interface ICreateMedicineService
{
    Task<ServiceResult<MedicineDto>> ExecuteAsync(CreateMedicineCommand command, CancellationToken cancellationToken = default);
}
