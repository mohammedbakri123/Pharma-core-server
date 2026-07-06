using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IAddPurchaseReturnItemService
{
    Task<ServiceResult<PurchaseReturnItemDto>> ExecuteAsync(AddPurchaseReturnItemCommand command, CancellationToken cancellationToken = default);
}
