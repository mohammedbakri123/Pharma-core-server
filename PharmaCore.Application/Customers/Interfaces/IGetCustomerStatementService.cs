using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IGetCustomerStatementService
{
    Task<ServiceResult<CustomerStatementDto>> ExecuteAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
