using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IListCustomersService
{
    Task<ServiceResult<PagedResult<CustomerDto>>> ExecuteAsync(ListCustomersQuery query, CancellationToken cancellationToken = default);
}
