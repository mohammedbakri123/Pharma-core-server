using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface ICancelPurchaseService
{
    Task<ServiceResult<bool>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default);
}
