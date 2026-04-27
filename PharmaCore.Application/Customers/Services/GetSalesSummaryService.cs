using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetSalesSummaryService(
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository,
    ISalesReturnRepository salesReturnRepository,
    ILogger<GetSalesSummaryService> logger)
    : IGetSalesSummaryService
{
    public async Task<ServiceResult<SalesSummaryDto>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            //total sales prices
            var totalSales = await saleRepository.GetTotalSalesAmountByCustomerIdAsync(customerId, cancellationToken);
        
            var sales = await saleRepository.GetByCustomerIdAsync(customerId, null, null, cancellationToken);
            var saleIds = sales.Select(s => s.SaleId).ToList();
        
            decimal totalPaid = 0;
            foreach (var saleId in saleIds)
            {
                //total paid amount
                totalPaid += await paymentRepository.GetTotalAmountByReferenceAsync(PaymentReferenceType.SALE, saleId, cancellationToken);
            }

            var totalReturns = await salesReturnRepository.GetTotalAmountByCustomerIdAsync(customerId, cancellationToken);

            var summary = new SalesSummaryDto(
                customerId,
                totalSales,
                totalPaid,
                totalReturns,
                totalSales - totalPaid - totalReturns);

            return ServiceResult<SalesSummaryDto>.Ok(summary);
        }
        catch (Exception e)
        {
            string errMesage = $"error getting customer dept: {e.Message}, {e.StackTrace}, {e.InnerException?.Message}";
            logger.LogError(e, errMesage);
            return ServiceResult<SalesSummaryDto>.Fail(ServiceErrorType.ServerError,errMesage);
        }
    }
}
