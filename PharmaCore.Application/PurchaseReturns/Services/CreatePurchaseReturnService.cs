using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class CreatePurchaseReturnService(
    IPurchaseRepository purchaseRepository,
    IPurchaseReturnRepository purchaseReturnRepository,
    IStockMovementRepository stockMovementRepository,
    IPaymentRepository paymentRepository,
    ILogger<CreatePurchaseReturnService> logger)
    : ICreatePurchaseReturnService
{
    public async Task<ServiceResult<PurchaseReturnDto>> ExecuteAsync(CreatePurchaseReturnCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await purchaseRepository.GetByIdWithItemsAsync(command.PurchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {command.PurchaseId} not found.");
            }

            if (command.Items.Count == 0)
            {
                return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.Validation, "At least one item is required for a return.");
            }

            var purchaseReturn = PurchaseReturn.Create(
                command.PurchaseId,
                purchase.SupplierId,
                command.UserId,
                command.Note);

            var created = await purchaseReturnRepository.AddAsync(purchaseReturn, cancellationToken);

            var stockMovements = new List<StockMovement>();
            var itemDtos = new List<PurchaseReturnItemDto>();

            foreach (var itemCmd in command.Items)
            {
                var item = PurchaseReturnItem.Create(
                    created.PurchaseReturnId,
                    itemCmd.PurchaseItemId,
                    itemCmd.BatchId,
                    itemCmd.Quantity,
                    itemCmd.UnitPrice);

                var createdItem = await purchaseReturnRepository.AddItemAsync(item, cancellationToken);
                itemDtos.Add(new PurchaseReturnItemDto(
                    createdItem.PurchaseReturnItemId,
                    createdItem.PurchaseItemId,
                    createdItem.BatchId,
                    createdItem.Quantity,
                    createdItem.UnitPrice,
                    createdItem.TotalPrice));

                stockMovements.Add(StockMovement.Create(
                    purchase.Items.First(i => i.PurchaseItemId == itemCmd.PurchaseItemId).MedicineId,
                    itemCmd.BatchId,
                    itemCmd.Quantity,
                    StockMovementType.OUT,
                    StockMovementReferenceType.RETURN,
                    created.PurchaseReturnId));
            }

            await purchaseReturnRepository.UpdateTotalAmountAsync(created.PurchaseReturnId, cancellationToken);
            await stockMovementRepository.AddRangeAsync(stockMovements, cancellationToken);

            int? refundPaymentId = null;

            if (command.RefundPayment != null)
            {
                var refundPayment = Payment.Create(
                    PaymentType.INCOMING,
                    PaymentReferenceType.PURCHASE_RETURN,
                    created.PurchaseReturnId,
                    command.RefundPayment.Method.HasValue ? (PaymentMethod?)command.RefundPayment.Method : null,
                    command.UserId,
                    created.TotalAmount,
                    command.RefundPayment.Description ?? $"Refund for purchase return {created.PurchaseReturnId}");

                var createdPayment = await paymentRepository.AddAsync(refundPayment, cancellationToken);
                refundPaymentId = createdPayment.PaymentId;

                logger.LogInformation("Refund payment {PaymentId} created for purchase return {ReturnId}",
                    refundPaymentId, created.PurchaseReturnId);
            }

            logger.LogInformation("Purchase return {ReturnId} created for purchase {PurchaseId} with {ItemCount} items",
                created.PurchaseReturnId, command.PurchaseId, command.Items.Count);

            return ServiceResult<PurchaseReturnDto>.Ok(
                new PurchaseReturnDto(
                    created.PurchaseReturnId,
                    created.PurchaseId,
                    created.SupplierId,
                    created.UserId,
                    created.TotalAmount,
                    created.Note,
                    created.CreatedAt,
                    itemDtos,
                    refundPaymentId));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating purchase return for purchase {PurchaseId}", command.PurchaseId);
            return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.ServerError, $"Error creating purchase return: {e.Message}");
        }
    }
}
