using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface IDeleteSalesReturnService
{
    Task<ServiceResult<bool>> ExecuteAsync(int salesReturnId, CancellationToken cancellationToken = default);
}