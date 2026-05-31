using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class UpdateCustomerService(ICustomerRepository customerRepository, ILogger<UpdateCustomerService> logger)
    : IUpdateCustomerService
{
    public async Task<ServiceResult<CustomerDto>> ExecuteAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);

            if (customer == null)
                return ServiceResult<CustomerDto>.Fail(ServiceErrorType.NotFound, $"Customer with ID {command.CustomerId} not found.");

            if (!string.IsNullOrWhiteSpace(command.Name) && await customerRepository.ExistsByNameAsync(command.Name, command.CustomerId, cancellationToken))
                return ServiceResult<CustomerDto>.Fail(ServiceErrorType.Duplicate, $"Customer with name '{command.Name}' already exists.");

            customer.Update(command.Name, command.PhoneNumber, command.Address, command.Note);

            var updated = await customerRepository.UpdateAsync(customer, cancellationToken);

            logger.LogInformation("Customer '{Name}' updated with ID {Id}", updated.Name, updated.CustomerId);

            return ServiceResult<CustomerDto>.Ok(MapToDto(updated));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating customer {CustomerId}", command.CustomerId);
            return ServiceResult<CustomerDto>.Fail(ServiceErrorType.ServerError, $"Error updating customer: {e.Message}");
        }
    }

    private static CustomerDto MapToDto(Domain.Entities.Customer c) =>
        new CustomerDto(c.CustomerId, c.Name, c.PhoneNumber, c.Address, c.Note, c.CreatedAt);
}
