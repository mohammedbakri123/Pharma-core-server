using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IGetPurchaseBalanceService
{
    Task<ServiceResult<PurchaseBalanceDto>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default);
}
