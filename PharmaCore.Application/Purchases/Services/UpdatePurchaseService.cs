using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class UpdatePurchaseService(IPurchaseRepository purchaseRepository, ILogger<UpdatePurchaseService> logger)
    : IUpdatePurchaseService
{
    public async Task<ServiceResult<PurchaseDto>> ExecuteAsync(UpdatePurchaseCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await purchaseRepository.GetByIdWithItemsAsync(command.PurchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {command.PurchaseId} not found.");
            }

            if (command.SupplierId.HasValue)
            {
                purchase.AssignSupplier(command.SupplierId.Value);
            }

            if (command.InvoiceNumber is not null)
            {
                purchase.SetInvoiceNumber(command.InvoiceNumber);
            }

            if (command.Note is not null)
            {
                purchase.UpdateNote(command.Note);
            }

            if (command.Status.HasValue)
            {
                // Status changes are handled by dedicated complete/cancel endpoints
                return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.Validation, "Use the complete or cancel endpoints to change purchase status.");
            }

            var updated = await purchaseRepository.UpdateAsync(purchase, cancellationToken);

            logger.LogInformation("Purchase {PurchaseId} updated successfully", updated.PurchaseId);

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
        catch (Exception e)
        {
            logger.LogError(e, "Error updating purchase {PurchaseId}", command.PurchaseId);
            return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.ServerError, $"Error updating purchase: {e.Message}");
        }
    }
}
