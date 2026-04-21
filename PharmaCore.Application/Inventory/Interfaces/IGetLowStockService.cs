using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetLowStockService
{
    Task<ServiceResult<IReadOnlyList<LowStockItemDto>>> ExecuteAsync(GetLowStockQuery query, CancellationToken cancellationToken = default);
}
