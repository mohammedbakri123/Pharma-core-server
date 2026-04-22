using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class CancelPurchaseService(IPurchaseRepository purchaseRepository, ILogger<CancelPurchaseService> logger)
    : ICancelPurchaseService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await purchaseRepository.GetByIdAsync(purchaseId, cancellationToken);

            if (purchase is null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {purchaseId} not found.");
            }

            purchase.Cancel();
            await purchaseRepository.UpdateAsync(purchase, cancellationToken);

            logger.LogInformation("Purchase {PurchaseId} cancelled", purchaseId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (InvalidOperationException e)
        {
            return ServiceResult<bool>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error cancelling purchase {PurchaseId}", purchaseId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error cancelling purchase: {e.Message}");
        }
    }
}
