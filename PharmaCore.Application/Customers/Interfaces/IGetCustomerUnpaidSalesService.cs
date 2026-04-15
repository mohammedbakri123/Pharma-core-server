using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IGetCustomerUnpaidSalesService
{
    Task<ServiceResult<IReadOnlyList<UnpaidSaleDto>>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default);
}
