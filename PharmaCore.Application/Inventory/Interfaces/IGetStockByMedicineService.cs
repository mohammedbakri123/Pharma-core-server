using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetStockByMedicineService
{
    Task<ServiceResult<StockWithBatchesDto>> ExecuteAsync(GetStockByMedicineQuery query, CancellationToken cancellationToken = default);
}
