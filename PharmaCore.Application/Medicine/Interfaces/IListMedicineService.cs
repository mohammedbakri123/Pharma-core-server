using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Interfaces;

public interface IListMedicineService
{
    Task<ServiceResult<PagedResult<MedicineDto>>> ExecuteAsync(ListMedicineQuery query, CancellationToken cancellationToken = default);
}
