using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface IUpdateSalesReturnItemService
{
    Task<ServiceResult<SalesReturnItemDto>> ExecuteAsync(UpdateSalesReturnItemCommand command, CancellationToken cancellationToken = default);
}