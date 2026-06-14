using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IStockAlertService
{
    Task<ServiceResult<PagedResult<StockAlertDto>>> ExecuteAsync(GetStockAlertQuery query, CancellationToken cancellationToken = default);
}
