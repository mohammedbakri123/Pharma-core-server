using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Interfaces;

public interface IListPurchaseReturnsService
{
    Task<ServiceResult<List<PurchaseReturnDto>>> ExecuteAsync(ListPurchaseReturnsQuery query, CancellationToken cancellationToken = default);
}
