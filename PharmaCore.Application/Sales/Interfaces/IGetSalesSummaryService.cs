using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface IGetSalesSummaryService
{
    Task<ServiceResult<SalesSummaryDto>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default);
}
