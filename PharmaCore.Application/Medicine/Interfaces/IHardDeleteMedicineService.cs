using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Interfaces;

public interface IHardDeleteMedicineService
{
    Task<ServiceResult<bool>> ExecuteAsync(DeleteMedicineCommand command, CancellationToken cancellationToken = default);
}