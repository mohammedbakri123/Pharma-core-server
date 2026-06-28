using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class CancelSalesReturnService(
    ISalesReturnRepository salesReturnRepository,
    ILogger<CancelSalesReturnService> logger)
    : ICancelSalesReturnService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await salesReturnRepository.GetByIdAsync(salesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            if (salesReturn.Status != SalesReturnStatus.Draft)
                return ServiceResult<bool>.Fail(ServiceErrorType.Validation, "Only draft sales returns can be cancelled.");

            salesReturn.Cancel();
            await salesReturnRepository.UpdateAsync(salesReturn, cancellationToken);

            logger.LogInformation("Cancelled sales return {SalesReturnId}", salesReturnId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error cancelling sales return {SalesReturnId}", salesReturnId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error cancelling sales return: {e.Message}");
        }
    }
}
