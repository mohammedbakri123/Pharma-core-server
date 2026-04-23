using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class ListPurchaseReturnsService(
    IPurchaseReturnRepository purchaseReturnRepository,
    ILogger<ListPurchaseReturnsService> logger)
    : IListPurchaseReturnsService
{
    public async Task<ServiceResult<List<PurchaseReturnDto>>> ExecuteAsync(ListPurchaseReturnsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var returns = await purchaseReturnRepository.ListAsync(cancellationToken);
            var filtered = returns.Where(r => r.PurchaseId == query.PurchaseId).ToList();

            var dtos = filtered.Select(r => new PurchaseReturnDto(
                r.PurchaseReturnId,
                r.PurchaseId,
                r.SupplierId,
                r.UserId,
                r.TotalAmount,
                r.Note,
                r.CreatedAt,
                r.Items.Select(i => new PurchaseReturnItemDto(
                    i.PurchaseReturnItemId,
                    i.PurchaseItemId,
                    i.BatchId,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice)).ToList()
            )).ToList();

            return ServiceResult<List<PurchaseReturnDto>>.Ok(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing purchase returns for purchase {PurchaseId}", query.PurchaseId);
            return ServiceResult<List<PurchaseReturnDto>>.Fail(ServiceErrorType.ServerError, $"Error listing returns: {e.Message}");
        }
    }
}
