using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IGetSalesSummaryService
{
    Task<ServiceResult<SalesSummaryDto>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default);
}
