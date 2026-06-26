using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface ICompletePurchaseReturnService
{
    Task<ServiceResult<CompletePurchaseReturnResultDto>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
}
