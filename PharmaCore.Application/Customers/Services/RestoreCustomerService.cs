using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class RestoreCustomerService(ICustomerRepository customerRepository, ILogger<RestoreCustomerService> logger)
    : IRestoreCustomerService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(RestoreCustomerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await customerRepository.RestoreDeletedAsync(command.CustomerId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Deleted customer with ID {command.CustomerId} not found or is not deleted.");
            }

            logger.LogInformation("Customer with ID {CustomerId} restored successfully", command.CustomerId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error restoring customer");
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error restoring customer: {e.Message}");
        }
    }
}
