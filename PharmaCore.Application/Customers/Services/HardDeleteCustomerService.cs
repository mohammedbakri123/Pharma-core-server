using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class HardDeleteCustomerService : IHardDeleteCustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<HardDeleteCustomerService> _logger;

    public HardDeleteCustomerService(ICustomerRepository customerRepository, ILogger<HardDeleteCustomerService> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

            if (customer == null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Customer with ID {customerId} not found.");
            }

            var result = await _customerRepository.HardDeleteAsync(customerId, cancellationToken);

            if (!result)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to permanently delete customer.");
            }

            _logger.LogInformation("Customer with ID {Id} permanently deleted", customerId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error permanently deleting customer {CustomerId}", customerId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error permanently deleting customer: {e.Message}");
        }
    }
}