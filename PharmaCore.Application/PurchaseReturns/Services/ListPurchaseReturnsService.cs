using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;
using PurchaseReturnEntity = PharmaCore.Domain.Entities.PurchaseReturn;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class ListPurchaseReturnsService(
    IPurchaseReturnRepository purchaseReturnRepository,
    ILogger<ListPurchaseReturnsService> logger)
    : IListPurchaseReturnsService
{
    public async Task<ServiceResult<PagedResult<PurchaseReturnListItemDto>>> ExecuteAsync(
        ListPurchaseReturnsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<PurchaseReturnListItemDto>>.Fail(
                    ServiceErrorType.Validation, "Page and limit must be greater than zero.");

            var result = await purchaseReturnRepository.ListPagedAsync(query, cancellationToken);

            var dtos = result.Items.Select(Map).ToList();

            return ServiceResult<PagedResult<PurchaseReturnListItemDto>>.Ok(
                new PagedResult<PurchaseReturnListItemDto>(dtos, result.Total, result.Page, result.Limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing purchase returns for purchase {PurchaseId}", query.PurchaseId);
            return ServiceResult<PagedResult<PurchaseReturnListItemDto>>.Fail(
                ServiceErrorType.ServerError, $"Error listing returns: {e.Message}");
        }
    }

    private static PurchaseReturnListItemDto Map(PurchaseReturnEntity r) => new(
        r.PurchaseReturnId,
        r.PurchaseId,
        r.SupplierId,
        r.UserId,
        r.Status,
        r.TotalAmount,
        r.Note,
        r.CreatedAt);
}
