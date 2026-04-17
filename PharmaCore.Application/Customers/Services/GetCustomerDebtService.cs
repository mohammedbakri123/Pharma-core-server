using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerDebtService : IGetCustomerDebtService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerFinancialRepository _customerFinancialRepository;
    private readonly ILogger<GetCustomerDebtService> _logger;

    public GetCustomerDebtService(
        ICustomerRepository customerRepository,
        ICustomerFinancialRepository customerFinancialRepository,
        ILogger<GetCustomerDebtService> logger)
    {
        _customerRepository = customerRepository;
        _customerFinancialRepository = customerFinancialRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<CustomerDebtDto>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                return ServiceResult<CustomerDebtDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            var debt = await _customerFinancialRepository.GetDebtAsync(customerId, cancellationToken);
            if (debt == null)
                return ServiceResult<CustomerDebtDto>.Fail(ServiceErrorType.NotFound, "No sales data found for this customer.");

            return ServiceResult<CustomerDebtDto>.Ok(debt);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting customer debt for {CustomerId}", customerId);
            return ServiceResult<CustomerDebtDto>.Fail(ServiceErrorType.ServerError, $"Error getting customer debt: {e.Message}");
        }
    }
}
