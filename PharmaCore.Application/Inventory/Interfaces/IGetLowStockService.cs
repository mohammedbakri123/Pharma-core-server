using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetLowStockService
{
    Task<ServiceResult<IReadOnlyList<LowStockItemDto>>> ExecuteAsync(int threshold, CancellationToken cancellationToken = default);
}