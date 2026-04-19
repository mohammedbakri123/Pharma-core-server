using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class DeleteCustomerService(ICustomerRepository customerRepository, ILogger<DeleteCustomerService> logger)
    : IDeleteCustomerService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(DeleteCustomerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);

            if (customer == null)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Customer with ID {command.CustomerId} not found.");

            customer.MarkDeleted();

            var result = await customerRepository.SoftDeleteAsync(command.CustomerId, cancellationToken);

            if (!result)
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to delete customer.");

            logger.LogInformation("Customer with ID {Id} deleted successfully", command.CustomerId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting customer {CustomerId}", command.CustomerId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error deleting customer: {e.Message}");
        }
    }
}
