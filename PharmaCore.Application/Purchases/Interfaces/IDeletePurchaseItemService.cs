using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IDeletePurchaseItemService
{
    Task<ServiceResult<bool>> ExecuteAsync(DeletePurchaseItemCommand command, CancellationToken cancellationToken = default);
}
