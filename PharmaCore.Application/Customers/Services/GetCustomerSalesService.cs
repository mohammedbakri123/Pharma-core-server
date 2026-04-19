using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerSalesService(ICustomerRepository customerRepository, ILogger<GetCustomerSalesService> logger)
    : IGetCustomerSalesService
{
    public async Task<ServiceResult<PagedResult<CustomerSaleDto>>> ExecuteAsync(int customerId, int page, int limit, short? status, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                return ServiceResult<PagedResult<CustomerSaleDto>>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            var sales = await customerRepository.GetSalesAsync(customerId, page, limit, status, cancellationToken);

            return ServiceResult<PagedResult<CustomerSaleDto>>.Ok(sales);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting customer sales for {CustomerId}", customerId);
            return ServiceResult<PagedResult<CustomerSaleDto>>.Fail(ServiceErrorType.ServerError, $"Error getting customer sales: {e.Message}");
        }
    }
}
