using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface ICompletePurchaseService
{
    Task<ServiceResult<CompletePurchaseResultDto>> ExecuteAsync(CompletePurchaseCommand command, CancellationToken cancellationToken = default);
}
