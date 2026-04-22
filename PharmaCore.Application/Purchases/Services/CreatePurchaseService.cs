using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class CreatePurchaseService(IPurchaseRepository purchaseRepository, ILogger<CreatePurchaseService> logger)
    : ICreatePurchaseService
{
    public async Task<ServiceResult<PurchaseDto>> ExecuteAsync(CreatePurchaseCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = Purchase.Create(command.SupplierId, command.InvoiceNumber, command.Note);
            var created = await purchaseRepository.AddAsync(purchase, cancellationToken);

            logger.LogInformation("Purchase created with ID {PurchaseId}", created.PurchaseId);

            return ServiceResult<PurchaseDto>.Ok(
                new PurchaseDto(
                    created.PurchaseId,
                    created.SupplierId,
                    null,
                    created.InvoiceNumber,
                    created.TotalAmount,
                    created.Status,
                    created.CreatedAt,
                    created.Note,
                    new List<PurchaseItemDto>()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating purchase");
            return ServiceResult<PurchaseDto>.Fail(ServiceErrorType.ServerError, $"Error creating purchase: {e.Message}");
        }
    }
}
