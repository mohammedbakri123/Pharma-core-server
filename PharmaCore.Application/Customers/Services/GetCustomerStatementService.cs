using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerStatementService(
    ICustomerRepository customerRepository,
    ILogger<GetCustomerStatementService> logger)
    : IGetCustomerStatementService
{
    public async Task<ServiceResult<CustomerStatementDto>> ExecuteAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                return ServiceResult<CustomerStatementDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            var statement = await customerRepository.GetStatementAsync(customerId, from, to, cancellationToken);

            return ServiceResult<CustomerStatementDto>.Ok(statement);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting customer statement for {CustomerId}", customerId);
            return ServiceResult<CustomerStatementDto>.Fail(ServiceErrorType.ServerError, $"Error getting customer statement: {e.Message}");
        }
    }
}
