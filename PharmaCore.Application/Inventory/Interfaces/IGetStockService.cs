using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetStockService
{
    Task<ServiceResult<PagedResult<StockItemDto>>> ExecuteAsync(int page, int limit, int? medicineId, CancellationToken cancellationToken = default);
}