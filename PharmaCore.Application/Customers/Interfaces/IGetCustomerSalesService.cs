using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IGetCustomerSalesService
{
    Task<ServiceResult<PagedResult<CustomerSaleDto>>> ExecuteAsync(int customerId, int page, int limit, short? status, CancellationToken cancellationToken = default);
}
