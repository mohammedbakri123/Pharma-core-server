using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Interfaces;

public interface IRestoreMedicineService
{
    Task<ServiceResult<bool>> ExecuteAsync(RestoreMedicineCommand command, CancellationToken cancellationToken = default);
}
