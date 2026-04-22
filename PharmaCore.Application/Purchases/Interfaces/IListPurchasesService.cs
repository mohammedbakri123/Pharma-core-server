using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IListPurchasesService
{
    Task<ServiceResult<PagedResult<PurchaseListItemDto>>> ExecuteAsync(ListPurchasesQuery query, CancellationToken cancellationToken = default);
}
