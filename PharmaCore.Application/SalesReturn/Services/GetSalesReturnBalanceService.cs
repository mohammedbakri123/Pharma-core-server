using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class GetSalesReturnBalanceService(
    ISalesReturnRepository salesReturnRepository,
    IPaymentRepository paymentRepository,
    ILogger<GetSalesReturnBalanceService> logger)
    : IGetSalesReturnBalanceService
{
    public async Task<ServiceResult<SalesReturnBalanceDto>> ExecuteAsync(int salesReturnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await salesReturnRepository.GetByIdAsync(salesReturnId, cancellationToken);
            if (salesReturn is null)
                return ServiceResult<SalesReturnBalanceDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            var paidAmount = await paymentRepository.GetTotalAmountByReferenceAsync(PaymentReferenceType.SALES_RETURN, salesReturnId, cancellationToken);
            var balance = new SalesReturnBalanceDto(
                salesReturn.SalesReturnId,
                salesReturn.TotalAmount,
                paidAmount,
                salesReturn.TotalAmount - paidAmount);

            return ServiceResult<SalesReturnBalanceDto>.Ok(balance);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting sales return balance for {SalesReturnId}", salesReturnId);
            return ServiceResult<SalesReturnBalanceDto>.Fail(ServiceErrorType.ServerError, $"Error getting sales return balance: {e.Message}");
        }
    }
}
