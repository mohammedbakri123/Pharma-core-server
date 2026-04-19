using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface IListSalesReturnService
{
    Task<ServiceResult<PagedResult<SalesReturnListItemDto>>> ExecuteAsync(ListSalesReturnQuery query, CancellationToken cancellationToken = default);
}