using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class UpdatePurchaseReturnService(
    IPurchaseReturnRepository purchaseReturnRepository,
    ILogger<UpdatePurchaseReturnService> logger)
    : IUpdatePurchaseReturnService
{
    public async Task<ServiceResult<PurchaseReturnDto>> ExecuteAsync(
        UpdatePurchaseReturnCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchaseReturn = await purchaseReturnRepository.GetByIdAsync(command.PurchaseReturnId, cancellationToken);
            if (purchaseReturn is null)
                return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.NotFound, "Purchase return not found.");

            purchaseReturn.UpdateNote(command.Note);
            var updated = await purchaseReturnRepository.UpdateAsync(purchaseReturn, cancellationToken);

            logger.LogInformation("Updated purchase return {PurchaseReturnId}", updated.PurchaseReturnId);

            return ServiceResult<PurchaseReturnDto>.Ok(new PurchaseReturnDto(
                updated.PurchaseReturnId,
                updated.PurchaseId,
                updated.SupplierId,
                updated.UserId,
                updated.Status,
                updated.TotalAmount,
                updated.Note,
                updated.CreatedAt,
                updated.Items.Select(i => new PurchaseReturnItemDto(
                    i.PurchaseReturnItemId,
                    i.PurchaseItemId,
                    i.BatchId,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice)).ToList(),
                null));
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Invalid operation updating purchase return");
            return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating purchase return {PurchaseReturnId}", command.PurchaseReturnId);
            return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}
