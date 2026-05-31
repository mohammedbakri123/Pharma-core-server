using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class ListPurchasesService(
    IPurchaseRepository purchaseRepository,
    ISupplierRepository supplierRepository,
    ILogger<ListPurchasesService> logger)
    : IListPurchasesService
{
    public async Task<ServiceResult<PagedResult<PurchaseListItemDto>>> ExecuteAsync(ListPurchasesQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var purchases = await purchaseRepository.ListAsync(cancellationToken);
            var filtered = purchases.AsQueryable();

            if (query.SupplierId.HasValue)
            {
                filtered = filtered.Where(p => p.SupplierId == query.SupplierId.Value);
            }

            if (query.Status.HasValue)
            {
                filtered = filtered.Where(p => p.Status == query.Status.Value);
            }

            if (query.From.HasValue)
            {
                filtered = filtered.Where(p => p.CreatedAt >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                var toDate = query.To.Value.Date.AddDays(1).AddTicks(-1);
                filtered = filtered.Where(p => p.CreatedAt <= toDate);
            }

            var total = filtered.Count();
            var items = filtered
                .OrderByDescending(p => p.CreatedAt)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToList();

            var supplierIds = items.Where(p => p.SupplierId.HasValue).Select(p => p.SupplierId!.Value).Distinct().ToList();
            var suppliers = await supplierRepository.ListAsync(cancellationToken);
            var supplierMap = suppliers.Where(s => supplierIds.Contains(s.SupplierId)).ToDictionary(s => s.SupplierId, s => s.Name);

            var dtos = items.Select(p => new PurchaseListItemDto(
                p.PurchaseId,
                p.SupplierId,
                p.SupplierId.HasValue && supplierMap.ContainsKey(p.SupplierId.Value) ? supplierMap[p.SupplierId.Value] : null,
                p.InvoiceNumber,
                p.TotalAmount,
                p.Status,
                p.CreatedAt,
                p.Note
            )).ToList();

            return ServiceResult<PagedResult<PurchaseListItemDto>>.Ok(
                new PagedResult<PurchaseListItemDto>(dtos, total, query.Page, query.Limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting purchase list");
            return ServiceResult<PagedResult<PurchaseListItemDto>>.Fail(ServiceErrorType.ServerError, $"Error getting purchase list: {e.Message}");
        }
    }
}
