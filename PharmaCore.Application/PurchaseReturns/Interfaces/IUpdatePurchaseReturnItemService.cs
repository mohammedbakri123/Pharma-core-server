using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IUpdatePurchaseReturnItemService
{
    Task<ServiceResult<PurchaseReturnItemDto>> ExecuteAsync(UpdatePurchaseReturnItemCommand command, CancellationToken cancellationToken = default);
}
