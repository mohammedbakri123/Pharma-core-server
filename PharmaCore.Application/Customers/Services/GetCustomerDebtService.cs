using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerDebtService(
    ICustomerRepository customerRepository,
    ILogger<GetCustomerDebtService> logger)
    : IGetCustomerDebtService
{
    public async Task<ServiceResult<CustomerDebtDto>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                return ServiceResult<CustomerDebtDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            var debt = await customerRepository.GetDebtAsync(customerId, cancellationToken);
            if (debt == null)
                return ServiceResult<CustomerDebtDto>.Fail(ServiceErrorType.NotFound, "No sales data found for this customer.");

            return ServiceResult<CustomerDebtDto>.Ok(debt);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting customer debt for {CustomerId}", customerId);
            return ServiceResult<CustomerDebtDto>.Fail(ServiceErrorType.ServerError, $"Error getting customer debt: {e.Message}");
        }
    }
}
