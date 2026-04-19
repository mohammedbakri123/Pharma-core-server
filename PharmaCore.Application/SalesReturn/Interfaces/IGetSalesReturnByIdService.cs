using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface IGetSalesReturnByIdService
{
    Task<ServiceResult<SalesReturnDetailsDto>> ExecuteAsync(GetSalesReturnByIdQuery query, CancellationToken cancellationToken = default);
}