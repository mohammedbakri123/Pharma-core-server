using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class CompletePurchaseService(
    IPurchaseRepository purchaseRepository,
    IPaymentRepository paymentRepository,
    IStockMovementRepository stockMovementRepository,
    ILogger<CompletePurchaseService> logger)
    : ICompletePurchaseService
{
    public async Task<ServiceResult<PurchaseDto>> ExecuteAsync(int purchaseId, int? userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await purchaseRepository.GetByIdWithItemsAsync(purchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {purchaseId} not found.");
            }

            if (purchase.Status != PurchaseStatus.DRAFT)
            {
                return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.Validation, "Only draft purchases can be completed.");
            }

            if (purchase.Items.Count == 0)
            {
                return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.Validation, "Cannot complete a purchase with no items.");
            }

            purchase.Complete();
            var updated = await purchaseRepository.UpdateAsync(purchase, cancellationToken);

            // Create stock movements for each item
            var stockMovements = purchase.Items.Select(item =>
                StockMovement.Create(
                    item.MedicineId,
                    item.BatchId,
                    item.Quantity,
                    StockMovementType.IN,
                    StockMovementReferenceType.PURCHASE,
                    purchaseId)).ToList();

            await stockMovementRepository.AddRangeAsync(stockMovements, cancellationToken);

            // Create payment IN
            var payment = Payment.Create(
                PaymentType.INCOMING,
                PaymentReferenceType.PURCHASE,
                purchaseId,
                null,
                userId,
                purchase.TotalAmount,
                $"Purchase #{purchaseId}");

            await paymentRepository.AddAsync(payment, cancellationToken);

            logger.LogInformation("Purchase {PurchaseId} completed with {ItemCount} items and payment IN", purchaseId, purchase.Items.Count);

            return ServiceResult<PurchaseDto>.Ok(
                new PurchaseDto(
                    updated.PurchaseId,
                    updated.SupplierId,
                    null,
                    updated.InvoiceNumber,
                    updated.TotalAmount,
                    updated.Status,
                    updated.CreatedAt,
                    updated.Note,
                    updated.Items.Select(i => new PurchaseItemDto(
                        i.PurchaseItemId, i.MedicineId, null, i.BatchId, null,
                        i.Quantity, i.PurchasePrice, i.SellPrice, i.TotalPrice, i.ExpireDate)).ToList()));
        }
        catch (InvalidOperationException e)
        {
            return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error completing purchase {PurchaseId}", purchaseId);
            return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.ServerError, $"Error completing purchase: {e.Message}");
        }
    }
}
