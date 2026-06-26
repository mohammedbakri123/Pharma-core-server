using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Interfaces;

public interface ICompleteSalesReturnService
{
    Task<ServiceResult<CompleteSalesReturnResultDto>> ExecuteAsync(int salesReturnId, CancellationToken cancellationToken = default);
}
