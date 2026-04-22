using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IGetPurchaseByIdService
{
    Task<ServiceResult<PurchaseDto>> ExecuteAsync(GetPurchaseByIdQuery query, CancellationToken cancellationToken = default);
}
