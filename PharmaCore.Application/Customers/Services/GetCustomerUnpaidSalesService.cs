using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerUnpaidSalesService(
    ICustomerRepository customerRepository,
    ILogger<GetCustomerUnpaidSalesService> logger)
    : IGetCustomerUnpaidSalesService
{
    public async Task<ServiceResult<IReadOnlyList<UnpaidSaleDto>>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            var unpaidSales = await customerRepository.GetUnpaidSalesAsync(customerId, cancellationToken);

            return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Ok(unpaidSales);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting unpaid sales for customer {CustomerId}", customerId);
            return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Fail(ServiceErrorType.ServerError, $"Error getting unpaid sales: {e.Message}");
        }
    }
}
