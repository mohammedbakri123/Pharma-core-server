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
                return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {command.PurchaseId} not found.");

            if (purchase.Status != PurchaseStatus.Completed)
                return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.Validation, "Can only create returns for completed purchases.");

            var purchaseReturn = PurchaseReturn.Create(command.PurchaseId, purchase.SupplierId, command.UserId, command.Note);
            var created = await purchaseReturnRepository.AddAsync(purchaseReturn, cancellationToken);

            //I am not sure if we need this line anymore
            // await purchaseReturnRepository.UpdateTotalAmountAsync(created.PurchaseReturnId, cancellationToken);

            logger.LogInformation("Purchase return {ReturnId} created for purchase {PurchaseId}",
                created.PurchaseReturnId, command.PurchaseId);

            return ServiceResult<PurchaseReturnDto>.Ok(
                new PurchaseReturnDto(created.PurchaseReturnId, created.PurchaseId, created.SupplierId,
                    created.UserId, created.Status, created.TotalAmount, created.Note, created.CreatedAt, null));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating purchase return for purchase {PurchaseId}", command.PurchaseId);
            return ServiceResult<PurchaseReturnDto>.Fail(ServiceErrorType.ServerError, $"Error creating purchase return: {e.Message}");
        }
    }
}
