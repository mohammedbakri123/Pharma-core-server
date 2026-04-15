using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class DeleteCustomerService : IDeleteCustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<DeleteCustomerService> _logger;

    public DeleteCustomerService(ICustomerRepository customerRepository, ILogger<DeleteCustomerService> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(DeleteCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);

        if (customer == null)
            return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Customer with ID {command.CustomerId} not found.");

        customer.MarkDeleted();

        var result = await _customerRepository.SoftDeleteAsync(command.CustomerId, cancellationToken);

        if (!result)
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to delete customer.");

        _logger.LogInformation("Customer with ID {Id} deleted successfully", command.CustomerId);

        return ServiceResult<bool>.Ok(true);
    }
}
