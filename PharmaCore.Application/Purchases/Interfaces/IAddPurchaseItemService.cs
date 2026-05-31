using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IAddPurchaseItemService
{
    Task<ServiceResult<PurchaseItemDto>> ExecuteAsync(AddPurchaseItemCommand command, CancellationToken cancellationToken = default);
}
