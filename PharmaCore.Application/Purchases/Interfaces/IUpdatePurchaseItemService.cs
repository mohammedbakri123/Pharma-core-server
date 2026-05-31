using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IUpdatePurchaseItemService
{
    Task<ServiceResult<PurchaseItemDto>> ExecuteAsync(UpdatePurchaseItemCommand command, CancellationToken cancellationToken = default);
}
