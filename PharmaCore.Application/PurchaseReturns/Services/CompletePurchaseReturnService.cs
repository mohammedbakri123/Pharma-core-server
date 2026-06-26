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

                stockMovements.Add(StockMovement.Create(
                    purchaseItem.MedicineId,
                    item.BatchId,
                    item.Quantity,
                    StockMovementType.OUT,
                    StockMovementReferenceType.RETURN,
                    purchaseReturn.PurchaseReturnId));
            }

            await stockMovementRepository.AddRangeAsync(stockMovements, cancellationToken);

            var refundPayment = Payment.Create(
                PaymentType.INCOMING,
                PaymentReferenceType.PURCHASE_RETURN,
                purchaseReturn.PurchaseReturnId,
                null,
                purchaseReturn.UserId,
                purchaseReturn.TotalAmount,
                $"Refund for purchase return {purchaseReturn.PurchaseReturnId}");

            var createdPayment = await paymentRepository.AddAsync(refundPayment, cancellationToken);

            purchaseReturn.Complete();
            var updated = await purchaseReturnRepository.UpdateAsync(purchaseReturn, cancellationToken);

            var result = new CompletePurchaseReturnResultDto(
                updated.PurchaseReturnId,
                updated.Status,
                updated.TotalAmount,
                DateTime.UtcNow,
                stockMovements.Count,
                createdPayment.PaymentId);

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
