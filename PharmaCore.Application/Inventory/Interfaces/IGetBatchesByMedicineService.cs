using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetBatchesByMedicineService
{
    Task<ServiceResult<IReadOnlyList<BatchStockDto>>> ExecuteAsync(GetBatchesByMedicineQuery query, CancellationToken cancellationToken = default);
}
