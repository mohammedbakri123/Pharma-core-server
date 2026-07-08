using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class CompletePurchaseReturnService(
    IPurchaseReturnRepository purchaseReturnRepository,
    IPurchaseRepository purchaseRepository,
    IBatchRepository batchRepository,
    IStockMovementRepository stockMovementRepository,
    IPaymentRepository paymentRepository,
    ILogger<CompletePurchaseReturnService> logger)
    : ICompletePurchaseReturnService
{
    public async Task<ServiceResult<CompletePurchaseReturnResultDto>> ExecuteAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchaseReturn = await purchaseReturnRepository.GetByIdWithItemsAsync(purchaseReturnId, cancellationToken);
            if (purchaseReturn is null)
                return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.NotFound, "Purchase return not found.");

            if (purchaseReturn.Status != PurchaseReturnStatus.DRAFT)
                return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.Validation, "Only draft purchase returns can be completed.");

            if (purchaseReturn.Items.Count == 0)
                return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.Validation, "Cannot complete a purchase return without items.");

            var purchase = await purchaseRepository.GetByIdWithItemsAsync(purchaseReturn.PurchaseId!.Value, cancellationToken);
            if (purchase is null)
                return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.NotFound, "Referenced purchase not found.");

            var stockMovements = new List<StockMovement>();

            foreach (var item in purchaseReturn.Items)
            {
                var purchaseItem = purchase.Items.FirstOrDefault(i => i.PurchaseItemId == item.PurchaseItemId);
                if (purchaseItem is null)
                    return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.NotFound, $"Purchase item {item.PurchaseItemId} not found.");

                var affected = await batchRepository.DecrementBatchStockAsync(item.BatchId, item.Quantity, cancellationToken);
                if (affected <= 0)
                    return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.Validation, "Cannot complete purchase return due to insufficient batch stock.");

                stockMovements.Add(StockMovement.Create(
                    purchaseItem.MedicineId,
                    item.BatchId,
                    item.Quantity,
                    StockMovementType.OUT,
                    StockMovementReferenceType.RETURN,
                    purchaseReturn.PurchaseReturnId));
            }

            await stockMovementRepository.AddRangeAsync(stockMovements, cancellationToken);

            purchaseReturn.Complete();
            var updated = await purchaseReturnRepository.UpdateAsync(purchaseReturn, cancellationToken);

            var result = new CompletePurchaseReturnResultDto(
                updated.PurchaseReturnId,
                updated.Status,
                updated.TotalAmount,
                DateTime.UtcNow,
                stockMovements.Count);

            return ServiceResult<CompletePurchaseReturnResultDto>.Ok(result);
        }
        catch (ArgumentException e)
        {
            return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (InvalidOperationException e)
        {
            return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error completing purchase return {PurchaseReturnId}", purchaseReturnId);
            return ServiceResult<CompletePurchaseReturnResultDto>.Fail(ServiceErrorType.ServerError, $"Error completing purchase return: {e.Message}");
        }
    }
}
