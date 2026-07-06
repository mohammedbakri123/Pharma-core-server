using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class PurchaseReturnItemValidator(
    IPurchaseReturnRepository purchaseReturnRepository,
    IPurchaseRepository purchaseRepository)
    : IPurchaseReturnItemValidator
{
    public async Task<PurchaseReturnItemValidationResult> ValidateAsync(
        int purchaseReturnId,
        int purchaseItemId,
        int quantity,
        int? excludePurchaseReturnItemId = null,
        CancellationToken cancellationToken = default)
    {
        var purchaseReturn = await purchaseReturnRepository.GetByIdWithItemsAsync(purchaseReturnId, cancellationToken);
        if (purchaseReturn is null)
            return PurchaseReturnItemValidationResult.Fail(ServiceErrorType.NotFound, "Purchase return not found.");

        if (purchaseReturn.Status != PurchaseReturnStatus.DRAFT)
            return PurchaseReturnItemValidationResult.Fail(ServiceErrorType.Validation, "Cannot modify a non-draft purchase return.");

        if (quantity <= 0)
            return PurchaseReturnItemValidationResult.Fail(ServiceErrorType.Validation, "Quantity must be greater than zero.");

        var purchaseItem = await purchaseRepository.GetItemByIdAsync(purchaseItemId, cancellationToken);
        if (purchaseItem is null)
            return PurchaseReturnItemValidationResult.Fail(ServiceErrorType.Validation, "Purchase item not found.");

        var currentDraftQuantity = purchaseReturn.Items
            .Where(i => i.PurchaseItemId == purchaseItemId && (excludePurchaseReturnItemId is null || i.PurchaseReturnItemId != excludePurchaseReturnItemId))
            .Sum(i => i.Quantity);

        var totalReturned = currentDraftQuantity + quantity;

        if (totalReturned > purchaseItem.Quantity)
            return PurchaseReturnItemValidationResult.Fail(ServiceErrorType.Validation,
                $"Return quantity exceeds original purchase quantity ({purchaseItem.Quantity}).");

        return PurchaseReturnItemValidationResult.Ok(purchaseItem);
    }
}
