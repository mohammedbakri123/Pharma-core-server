using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface ICancelPurchaseReturnService
{
    Task<ServiceResult<bool>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
}
