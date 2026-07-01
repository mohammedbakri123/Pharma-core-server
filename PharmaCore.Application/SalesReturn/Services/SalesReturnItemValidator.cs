using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class SalesReturnItemValidator(
    ISalesReturnRepository salesReturnRepository,
    ISaleRepository saleRepository)
    : ISalesReturnItemValidator
{
    public async Task<SalesReturnItemValidationResult> ValidateAsync(
        int salesReturnId,
        int saleItemId,
        int quantity,
        int? excludeSalesReturnItemId = null,
        CancellationToken cancellationToken = default)
    {
        var salesReturn = await salesReturnRepository.GetByIdWithItemsAsync(salesReturnId, cancellationToken);
        if (salesReturn is null)
            return SalesReturnItemValidationResult.Fail(ServiceErrorType.NotFound, "Sales return not found.");

        if (salesReturn.Status != SalesReturnStatus.Draft)
            return SalesReturnItemValidationResult.Fail(ServiceErrorType.Validation, "Cannot modify a non-draft sales return.");

        if (quantity <= 0)
            return SalesReturnItemValidationResult.Fail(ServiceErrorType.Validation, "Quantity must be greater than zero.");

        var saleItem = await saleRepository.GetItemByIdAsync(saleItemId, cancellationToken);
        if (saleItem is null)
            return SalesReturnItemValidationResult.Fail(ServiceErrorType.Validation, "Sale item not found.");

        var currentDraftQuantity = salesReturn.Items
            .Where(i => i.SaleItemId == saleItemId && (excludeSalesReturnItemId is null || i.SalesReturnItemId != excludeSalesReturnItemId))
            .Sum(i => i.Quantity);

        var completedReturnQuantity = await salesReturnRepository
            .GetCompletedReturnQuantityBySaleItemAsync(saleItemId, cancellationToken);

        var totalReturned = currentDraftQuantity + completedReturnQuantity + quantity;

        if (totalReturned > saleItem.Quantity)
            return SalesReturnItemValidationResult.Fail(ServiceErrorType.Validation,
                $"Return quantity exceeds original sale quantity ({saleItem.Quantity}).");

        return SalesReturnItemValidationResult.Ok(saleItem);
    }
}
