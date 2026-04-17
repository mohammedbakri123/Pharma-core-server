using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface ICompleteSaleService
{
    Task<ServiceResult<CompleteSaleResultDto>> ExecuteAsync(CompleteSaleCommand command, CancellationToken cancellationToken = default);
}