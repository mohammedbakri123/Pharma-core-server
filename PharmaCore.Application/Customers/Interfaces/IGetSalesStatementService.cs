using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IGetSalesStatementService
{
    Task<ServiceResult<SalesStatementDto>> ExecuteAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
