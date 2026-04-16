using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerSalesService : IGetCustomerSalesService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<GetCustomerSalesService> _logger;

    public GetCustomerSalesService(ICustomerRepository customerRepository, ILogger<GetCustomerSalesService> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<CustomerSaleDto>>> ExecuteAsync(int customerId, int page, int limit, short? status, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                return ServiceResult<PagedResult<CustomerSaleDto>>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            var sales = await _customerRepository.GetSalesAsync(customerId, page, limit, status, cancellationToken);

            return ServiceResult<PagedResult<CustomerSaleDto>>.Ok(sales);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting customer sales for {CustomerId}", customerId);
            return ServiceResult<PagedResult<CustomerSaleDto>>.Fail(ServiceErrorType.ServerError, $"Error getting customer sales: {e.Message}");
        }
    }
}
