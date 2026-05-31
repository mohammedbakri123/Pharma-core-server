using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IListDeletedCustomersService
{
    Task<ServiceResult<PagedResult<CustomerDto>>> ExecuteAsync(ListDeletedCustomersQuery query, CancellationToken cancellationToken = default);
}
