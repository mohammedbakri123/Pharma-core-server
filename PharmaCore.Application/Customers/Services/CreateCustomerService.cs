using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class CreateCustomerService(ICustomerRepository customerRepository, ILogger<CreateCustomerService> logger)
    : ICreateCustomerService
{
    public async Task<ServiceResult<CustomerDto>> ExecuteAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                return ServiceResult<CustomerDto>.Fail(ServiceErrorType.Validation, "Name is required.");

            if (await customerRepository.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken))
                return ServiceResult<CustomerDto>.Fail(ServiceErrorType.Duplicate, $"Customer with name '{command.Name}' already exists.");

            var entity = Domain.Entities.Customer.Create(command.Name, command.PhoneNumber, command.Address, command.Note);
            var created = await customerRepository.AddAsync(entity, cancellationToken);

            logger.LogInformation("Customer '{Name}' created with ID {Id}", created.Name, created.CustomerId);

            return ServiceResult<CustomerDto>.Ok(MapToDto(created));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating customer");
            return ServiceResult<CustomerDto>.Fail(ServiceErrorType.ServerError, $"Error creating customer: {e.Message}");
        }
    }

    private static CustomerDto MapToDto(Domain.Entities.Customer c) =>
        new CustomerDto(c.CustomerId, c.Name, c.PhoneNumber, c.Address, c.Note, c.CreatedAt);
}
