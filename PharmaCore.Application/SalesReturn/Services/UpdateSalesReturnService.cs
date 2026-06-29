using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class UpdateSalesReturnService(
    ISalesReturnRepository salesReturnRepository,
    ILogger<UpdateSalesReturnService> logger)
    : IUpdateSalesReturnService
{
    public async Task<ServiceResult<SalesReturnDto>> ExecuteAsync(UpdateSalesReturnCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await salesReturnRepository.GetByIdAsync(command.SalesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            salesReturn.UpdateNote(command.Note);

            var updated = await salesReturnRepository.UpdateAsync(salesReturn, cancellationToken);

            logger.LogInformation("Updated sales return {SalesReturnId}", updated.SalesReturnId);

            return ServiceResult<SalesReturnDto>.Ok(new SalesReturnDto(
                updated.SalesReturnId,
                updated.SaleId,
                updated.CustomerId,
                updated.UserId,
                null,
                updated.Status,
                updated.TotalAmount,
                updated.Note,
                updated.CreatedAt));
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Invalid operation updating sales return");
            return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating sales return {SalesReturnId}", command.SalesReturnId);
            return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}