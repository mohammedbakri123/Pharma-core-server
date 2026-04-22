using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IDeletePurchaseService
{
    Task<ServiceResult<bool>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default);
}
