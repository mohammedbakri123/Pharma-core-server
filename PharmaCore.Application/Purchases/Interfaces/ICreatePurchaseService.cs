using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface ICreatePurchaseService
{
    Task<ServiceResult<PurchaseDto>> ExecuteAsync(CreatePurchaseCommand command, CancellationToken cancellationToken = default);
}
