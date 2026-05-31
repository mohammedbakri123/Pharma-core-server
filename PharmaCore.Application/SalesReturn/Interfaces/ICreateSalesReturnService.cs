using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface ICreateSalesReturnService
{
    Task<ServiceResult<SalesReturnDto>> ExecuteAsync(CreateSalesReturnCommand command, CancellationToken cancellationToken = default);
}