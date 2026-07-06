using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IDeletePurchaseReturnService
{
    Task<ServiceResult<bool>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
}
