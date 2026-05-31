using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface IUpdateSalesReturnService
{
    Task<ServiceResult<SalesReturnDto>> ExecuteAsync(UpdateSalesReturnCommand command, CancellationToken cancellationToken = default);
}