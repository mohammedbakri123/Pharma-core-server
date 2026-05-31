using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface ICreatePurchaseReturnService
{
    Task<ServiceResult<PurchaseReturnDto>> ExecuteAsync(CreatePurchaseReturnCommand command, CancellationToken cancellationToken = default);
}
