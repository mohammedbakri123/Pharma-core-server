using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Interfaces;

public interface IUpdatePurchaseService
{
    Task<ServiceResult<PurchaseDto>> ExecuteAsync(UpdatePurchaseCommand command, CancellationToken cancellationToken = default);
}
