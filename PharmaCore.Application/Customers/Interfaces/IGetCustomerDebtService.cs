using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IGetCustomerDebtService
{
    Task<ServiceResult<CustomerDebtDto>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default);
}
