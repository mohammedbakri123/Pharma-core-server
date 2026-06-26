using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface ICancelSalesReturnService
{
    Task<ServiceResult<bool>> ExecuteAsync(int salesReturnId, CancellationToken cancellationToken = default);
}
