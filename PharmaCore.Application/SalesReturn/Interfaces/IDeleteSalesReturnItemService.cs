using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface IDeleteSalesReturnItemService
{
    Task<ServiceResult<bool>> ExecuteAsync(DeleteSalesReturnItemCommand command, CancellationToken cancellationToken = default);
}