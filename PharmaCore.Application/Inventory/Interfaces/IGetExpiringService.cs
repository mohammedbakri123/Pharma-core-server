using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetExpiringService
{
    Task<ServiceResult<IReadOnlyList<ExpiringItemDto>>> ExecuteAsync(GetExpiringQuery query, CancellationToken cancellationToken = default);
}
