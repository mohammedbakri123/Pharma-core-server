using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface IListSalesService
{
    Task<ServiceResult<PagedResult<SaleListItemDto>>> ExecuteAsync(ListSalesQuery query, CancellationToken cancellationToken = default);
}