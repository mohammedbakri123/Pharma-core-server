using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetStockByMedicineService
{
    Task<ServiceResult<StockWithBatchesDto>> ExecuteAsync(int medicineId, CancellationToken cancellationToken = default);
}