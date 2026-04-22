using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface ICompletePurchaseService
{
    Task<ServiceResult<PurchaseDto>> ExecuteAsync(int purchaseId, int? userId, CancellationToken cancellationToken = default);
}
