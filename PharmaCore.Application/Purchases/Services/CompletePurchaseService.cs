using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
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
    public async Task<ServiceResult<CompletePurchaseResultDto>> ExecuteAsync(CompletePurchaseCommand command,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var purchase = await purchaseRepository.GetByIdWithItemsAsync(command.PurchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<CompletePurchaseResultDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {command.PurchaseId} not found.");
            }

            if (purchase.Status != PurchaseStatus.Draft)
            {
                return ServiceResult<CompletePurchaseResultDto>.Fail(ServiceErrorType.Validation, "Only draft purchases can be completed.");
            }

            if (purchase.Items.Count == 0)
            {
                return ServiceResult<CompletePurchaseResultDto>.Fail(ServiceErrorType.Validation, "Cannot complete a purchase with no items.");
            }

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

            var stockMovements = purchase.Items.Select(item =>
                StockMovement.Create(
                    item.MedicineId,
                    item.BatchId ?? 0,
                    item.Quantity,
                    StockMovementType.IN,
                    StockMovementReferenceType.PURCHASE,
                    command.PurchaseId)).ToList();

            await stockMovementRepository.AddRangeAsync(stockMovements, cancellationToken);

            var payment = Payment.Create(
                PaymentType.OUTGOING,
                PaymentReferenceType.PURCHASE,
                command.PurchaseId,
                null,
                command.UserId,
                purchase.TotalAmount,
                $"Purchase #{command.PurchaseId}");

            var createdPayment = await paymentRepository.AddAsync(payment, cancellationToken);

            await tx.CommitAsync(cancellationToken);

            logger.LogInformation("Purchase {PurchaseId} completed with {ItemCount} items, stock movements, and payment OUT", command.PurchaseId, purchase.Items.Count);

            return ServiceResult<CompletePurchaseResultDto>.Ok(
                new CompletePurchaseResultDto(
                    updated.PurchaseId,
                    updated.Status,
                    updated.TotalAmount,
                    DateTime.UtcNow,
                    stockMovements.Count,
                    createdPayment.PaymentId));
        }
        catch (InvalidOperationException e)
        {
            await tx.RollbackAsync(cancellationToken);
            return ServiceResult<CompletePurchaseResultDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            await tx.RollbackAsync(cancellationToken);
            logger.LogError(e, "Error completing purchase {PurchaseId}", command.PurchaseId);
            return ServiceResult<CompletePurchaseResultDto>.Fail(ServiceErrorType.ServerError, $"Error completing purchase: {e.Message}");
        }
    }
}
