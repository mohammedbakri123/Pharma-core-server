using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerStatementService : IGetCustomerStatementService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerFinancialRepository _customerFinancialRepository;
    private readonly ILogger<GetCustomerStatementService> _logger;

    public GetCustomerStatementService(
        ICustomerRepository customerRepository,
        ICustomerFinancialRepository customerFinancialRepository,
        ILogger<GetCustomerStatementService> logger)
    {
        _customerRepository = customerRepository;
        _customerFinancialRepository = customerFinancialRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<CustomerStatementDto>> ExecuteAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                return ServiceResult<CustomerStatementDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            var statement = await _customerFinancialRepository.GetStatementAsync(customerId, from, to, cancellationToken);

            return ServiceResult<CustomerStatementDto>.Ok(statement);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting customer statement for {CustomerId}", customerId);
            return ServiceResult<CustomerStatementDto>.Fail(ServiceErrorType.ServerError, $"Error getting customer statement: {e.Message}");
        }
    }
}
