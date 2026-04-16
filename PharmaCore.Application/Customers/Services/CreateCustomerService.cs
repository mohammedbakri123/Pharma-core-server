using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class CreateCustomerService : ICreateCustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CreateCustomerService> _logger;

    public CreateCustomerService(ICustomerRepository customerRepository, ILogger<CreateCustomerService> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<CustomerDto>> ExecuteAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                return ServiceResult<CustomerDto>.Fail(ServiceErrorType.Validation, "Name is required.");

            var entity = Domain.Entities.Customer.Create(command.Name, command.PhoneNumber, command.Address, command.Note);
            var created = await _customerRepository.AddAsync(entity, cancellationToken);

            _logger.LogInformation("Customer '{Name}' created with ID {Id}", created.Name, created.CustomerId);

            return ServiceResult<CustomerDto>.Ok(MapToDto(created));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating customer");
            return ServiceResult<CustomerDto>.Fail(ServiceErrorType.ServerError, $"Error creating customer: {e.Message}");
        }
    }

    private static CustomerDto MapToDto(Domain.Entities.Customer c) =>
        new CustomerDto(c.CustomerId, c.Name, c.PhoneNumber, c.Address, c.Note, c.CreatedAt);
}
