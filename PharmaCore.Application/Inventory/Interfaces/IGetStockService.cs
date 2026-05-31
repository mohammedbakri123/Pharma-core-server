using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetStockService
{
    Task<ServiceResult<PagedResult<StockItemDto>>> ExecuteAsync(GetStockQuery query, CancellationToken cancellationToken = default);
}
