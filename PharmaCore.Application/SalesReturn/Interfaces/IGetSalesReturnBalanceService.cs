using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface IGetSalesReturnBalanceService
{
    Task<ServiceResult<SalesReturnBalanceDto>> ExecuteAsync(int salesReturnId, CancellationToken cancellationToken = default);
}
