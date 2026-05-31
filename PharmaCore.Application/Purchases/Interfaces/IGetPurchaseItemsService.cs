using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IGetPurchaseItemsService
{
    Task<ServiceResult<IReadOnlyList<PurchaseItemDto>>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default);
}
