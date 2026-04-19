using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface IAddSalesReturnItemService
{
    Task<ServiceResult<SalesReturnItemDto>> ExecuteAsync(AddSalesReturnItemCommand command, CancellationToken cancellationToken = default);
}