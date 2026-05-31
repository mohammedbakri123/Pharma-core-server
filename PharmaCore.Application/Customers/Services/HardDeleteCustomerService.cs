using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class HardDeleteCustomerService(
    ICustomerRepository customerRepository,
    ILogger<HardDeleteCustomerService> logger)
    : IHardDeleteCustomerService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);

            if (customer == null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Customer with ID {customerId} not found.");
            }

            var result = await customerRepository.HardDeleteAsync(customerId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to permanently delete customer.");
            }

            logger.LogInformation("Customer with ID {Id} permanently deleted", customerId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error permanently deleting customer {CustomerId}", customerId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error permanently deleting customer: {e.Message}");
        }
    }
}