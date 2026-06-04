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
    IBatchRepository batchRepository,
    IPaymentRepository paymentRepository,
    IStockMovementRepository stockMovementRepository,
    IUnitOfWork unitOfWork,
    ILogger<CompletePurchaseService> logger)
    : ICompletePurchaseService
{
    public async Task<ServiceResult<PurchaseDto>> ExecuteAsync(int purchaseId, int? userId,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

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

            // Mint a Batch for each item now that the purchase is being committed.
            // Until this point a draft item only carries its batch metadata; no Batch row exists.
            foreach (var item in purchase.Items)
            {
                var batch = Batch.Create(
                    item.MedicineId,
                    item.BatchNumber,
                    item.Quantity,
                    item.PurchasePrice,
                    item.SellPrice,
                    item.ExpireDate);

                var createdBatch = await batchRepository.AddAsync(batch, cancellationToken);
                item.AssignBatch(createdBatch.BatchId);
                await purchaseRepository.UpdateItemAsync(item, cancellationToken);
            }

            purchase.Complete();
            var updated = await purchaseRepository.UpdateAsync(purchase, cancellationToken);

            // Create stock movements for each item
            var stockMovements = purchase.Items.Select(item =>
                StockMovement.Create(
                    item.MedicineId,
                    item.BatchId ?? 0,
                    item.Quantity,
                    StockMovementType.IN,
                    StockMovementReferenceType.PURCHASE,
                    purchaseId)).ToList();

            await stockMovementRepository.AddRangeAsync(stockMovements, cancellationToken);
            //TODO: we need to create expense here too.

            // Create payment OUT
            var payment = Payment.Create(
                PaymentType.OUTGOING,
                PaymentReferenceType.PURCHASE,
                purchaseId,
                null,
                userId,
                purchase.TotalAmount,
                $"Purchase #{purchaseId}");

            await paymentRepository.AddAsync(payment, cancellationToken);

            await tx.CommitAsync(cancellationToken);

            logger.LogInformation("Purchase {PurchaseId} completed with {ItemCount} items and payment OUT", purchaseId, purchase.Items.Count);

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
                        i.PurchaseItemId, i.MedicineId, null, i.BatchId, i.BatchNumber,
                        i.Quantity, i.PurchasePrice, i.SellPrice, i.TotalPrice, i.ExpireDate)).ToList()));
        }
        catch (InvalidOperationException e)
        {
            await tx.RollbackAsync(cancellationToken);
            return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            await tx.RollbackAsync(cancellationToken);
            logger.LogError(e, "Error completing purchase {PurchaseId}", purchaseId);
            return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.ServerError, $"Error completing purchase: {e.Message}");
        }
    }
}
