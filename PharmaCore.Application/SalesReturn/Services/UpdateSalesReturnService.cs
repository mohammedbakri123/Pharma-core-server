using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class UpdateSalesReturnService : IUpdateSalesReturnService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly ILogger<UpdateSalesReturnService> _logger;

    public UpdateSalesReturnService(ISalesReturnRepository salesReturnRepository, ILogger<UpdateSalesReturnService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SalesReturnDto>> ExecuteAsync(UpdateSalesReturnCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await _salesReturnRepository.GetByIdAsync(command.SalesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            salesReturn.UpdateNote(command.Note);

            var updated = await _salesReturnRepository.UpdateAsync(salesReturn, cancellationToken);

            _logger.LogInformation("Updated sales return {SalesReturnId}", updated.SalesReturnId);

            return ServiceResult<SalesReturnDto>.Ok(new SalesReturnDto(
                updated.SalesReturnId,
                updated.SaleId,
                updated.CustomerId,
                updated.UserId,
                updated.TotalAmount,
                updated.Note,
                updated.CreatedAt));
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning(e, "Invalid operation updating sales return");
            return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating sales return {SalesReturnId}", command.SalesReturnId);
            return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}