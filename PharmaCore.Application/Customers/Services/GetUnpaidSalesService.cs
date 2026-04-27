using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;
using PharmaCore.Application.Customers.Dtos;

namespace PharmaCore.Application.Customers.Services;

public class GetUnpaidSalesService(ISaleRepository saleRepository, ILogger<GetUnpaidSalesService> logger) : IGetUnpaidSalesService
{
    public async Task<ServiceResult<IReadOnlyList<UnpaidSaleDto>>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var unpaidSales = await saleRepository.GetUnpaidSalesByCustomerIdAsync(customerId, cancellationToken);
            return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Ok(unpaidSales);
        }
        catch (Exception e)
        {
            string errMessage = $"error geting  unpaid sales: {e.Message}, {e.StackTrace}, {e.InnerException?.Message}";
            logger.LogError(e, errMessage);
            return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Fail(ServiceErrorType.ServerError,errMessage);
        }
    }
}
