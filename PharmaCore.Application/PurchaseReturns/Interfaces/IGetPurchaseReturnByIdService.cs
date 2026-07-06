using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IGetPurchaseReturnByIdService
{
    Task<ServiceResult<PurchaseReturnDetailsDto>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
}
