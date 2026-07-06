using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class GetPurchaseReturnByIdService(
    IPurchaseReturnRepository purchaseReturnRepository,
    ILogger<GetPurchaseReturnByIdService> logger)
    : IGetPurchaseReturnByIdService
{
    public async Task<ServiceResult<PurchaseReturnDetailsDto>> ExecuteAsync(
        int purchaseReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchaseReturn = await purchaseReturnRepository.GetByIdWithItemsAsync(purchaseReturnId, cancellationToken);
            if (purchaseReturn is null)
                return ServiceResult<PurchaseReturnDetailsDto>.Fail(ServiceErrorType.NotFound, "Purchase return not found.");

            var dto = new PurchaseReturnDetailsDto(
                purchaseReturn.PurchaseReturnId,
                purchaseReturn.PurchaseId,
                purchaseReturn.SupplierId,
                purchaseReturn.UserId,
                purchaseReturn.Status,
                purchaseReturn.TotalAmount,
                purchaseReturn.Note,
                purchaseReturn.CreatedAt,
                purchaseReturn.Items.Select(i => new PurchaseReturnItemDto(
                    i.PurchaseReturnItemId,
                    i.PurchaseItemId,
                    i.BatchId,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice)).ToList());

            return ServiceResult<PurchaseReturnDetailsDto>.Ok(dto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting purchase return {PurchaseReturnId}", purchaseReturnId);
            return ServiceResult<PurchaseReturnDetailsDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}
