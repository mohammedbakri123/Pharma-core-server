using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Interfaces;

public interface IGetMedicineByIdService
{
    Task<ServiceResult<MedicineDto>> ExecuteAsync(GetMedicineByIdQuery query, CancellationToken cancellationToken = default);
}
