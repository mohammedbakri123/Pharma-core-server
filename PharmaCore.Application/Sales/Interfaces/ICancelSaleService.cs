using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface ICancelSaleService
{
    Task<ServiceResult<bool>> ExecuteAsync(int saleId, CancellationToken cancellationToken = default);
}