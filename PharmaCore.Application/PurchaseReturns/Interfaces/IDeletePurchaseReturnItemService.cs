using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IDeletePurchaseReturnItemService
{
    Task<ServiceResult<bool>> ExecuteAsync(DeletePurchaseReturnItemCommand command, CancellationToken cancellationToken = default);
}
