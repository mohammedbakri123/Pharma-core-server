using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface IGetSaleBalanceService
{
    Task<ServiceResult<SaleBalanceDto>> ExecuteAsync(int saleId, CancellationToken cancellationToken = default);
}