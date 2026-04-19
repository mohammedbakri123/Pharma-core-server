using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetExpiringService
{
    Task<ServiceResult<IReadOnlyList<ExpiringItemDto>>> ExecuteAsync(int daysUntilExpiry, CancellationToken cancellationToken = default);
}