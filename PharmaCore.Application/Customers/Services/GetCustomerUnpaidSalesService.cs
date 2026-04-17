using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerUnpaidSalesService : IGetCustomerUnpaidSalesService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerFinancialRepository _customerFinancialRepository;
    private readonly ILogger<GetCustomerUnpaidSalesService> _logger;

    public GetCustomerUnpaidSalesService(
        ICustomerRepository customerRepository,
        ICustomerFinancialRepository customerFinancialRepository,
        ILogger<GetCustomerUnpaidSalesService> logger)
    {
        _customerRepository = customerRepository;
        _customerFinancialRepository = customerFinancialRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<UnpaidSaleDto>>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            var unpaidSales = await _customerFinancialRepository.GetUnpaidSalesAsync(customerId, cancellationToken);

            return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Ok(unpaidSales);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting unpaid sales for customer {CustomerId}", customerId);
            return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Fail(ServiceErrorType.ServerError, $"Error getting unpaid sales: {e.Message}");
        }
    }
}
