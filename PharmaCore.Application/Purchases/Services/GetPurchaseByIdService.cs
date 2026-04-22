using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class GetPurchaseByIdService(
    IPurchaseRepository purchaseRepository,
    ISupplierRepository supplierRepository,
    IMedicineRepository medicineRepository,
    IBatchRepository batchRepository,
    ILogger<GetPurchaseByIdService> logger)
    : IGetPurchaseByIdService
{
    public async Task<ServiceResult<PurchaseDto>> ExecuteAsync(GetPurchaseByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await purchaseRepository.GetByIdWithItemsAsync(query.PurchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {query.PurchaseId} not found.");
            }

            string? supplierName = null;
            if (purchase.SupplierId.HasValue)
            {
                var supplier = await supplierRepository.GetByIdAsync(purchase.SupplierId.Value, cancellationToken);
                supplierName = supplier?.Name;
            }

            var itemDtos = new List<PurchaseItemDto>();
            foreach (var item in purchase.Items)
            {
                var medicine = await medicineRepository.GetByIdAsync(item.MedicineId, cancellationToken);
                var batch = await batchRepository.GetByIdAsync(item.BatchId, cancellationToken);

                itemDtos.Add(new PurchaseItemDto(
                    item.PurchaseItemId,
                    item.MedicineId,
                    medicine?.Name,
                    item.BatchId,
                    batch?.BatchNumber,
                    item.Quantity,
                    item.PurchasePrice,
                    item.SellPrice,
                    item.TotalPrice,
                    item.ExpireDate));
            }

            return ServiceResult<PurchaseDto>.Ok(
                new PurchaseDto(
                    purchase.PurchaseId,
                    purchase.SupplierId,
                    supplierName,
                    purchase.InvoiceNumber,
                    purchase.TotalAmount,
                    purchase.Status,
                    purchase.CreatedAt,
                    purchase.Note,
                    itemDtos));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting purchase by ID {PurchaseId}", query.PurchaseId);
            return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.ServerError, $"Error getting purchase: {e.Message}");
        }
    }
}
