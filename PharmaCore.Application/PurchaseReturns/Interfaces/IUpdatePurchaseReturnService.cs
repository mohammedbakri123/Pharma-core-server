using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IUpdatePurchaseReturnService
{
    Task<ServiceResult<PurchaseReturnDto>> ExecuteAsync(UpdatePurchaseReturnCommand command, CancellationToken cancellationToken = default);
}
