using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IGetPurchaseReturnBalanceService
{
    Task<ServiceResult<PurchaseReturnBalanceDto>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
}
