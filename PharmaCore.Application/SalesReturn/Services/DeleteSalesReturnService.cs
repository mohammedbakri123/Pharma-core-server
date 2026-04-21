using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class DeleteSalesReturnService : IDeleteSalesReturnService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly ILogger<DeleteSalesReturnService> _logger;

    public DeleteSalesReturnService(ISalesReturnRepository salesReturnRepository, ILogger<DeleteSalesReturnService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _salesReturnRepository.SoftDeleteAsync(salesReturnId, cancellationToken);
            
            if (!success)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            _logger.LogInformation("Deleted sales return {SalesReturnId}", salesReturnId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting sales return {SalesReturnId}", salesReturnId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}