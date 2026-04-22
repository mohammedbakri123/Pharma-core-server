using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Interfaces;

public interface IListSuppliersService
{
    Task<ServiceResult<PagedResult<SupplierDto>>> ExecuteAsync(ListSuppliersQuery query, CancellationToken cancellationToken = default);
}
