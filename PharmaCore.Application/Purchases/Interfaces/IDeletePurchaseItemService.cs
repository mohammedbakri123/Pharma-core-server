using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IDeletePurchaseItemService
{
    Task<ServiceResult<bool>> ExecuteAsync(int purchaseId, int itemId, CancellationToken cancellationToken = default);
}
