using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class GetSalesReturnBalanceService(
    ISalesReturnRepository salesReturnRepository,
    ISaleRepository saleRepository,
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

            var maxRefundable = await ComputeMaxRefundableAsync(salesReturn, cancellationToken);
            var remainingAmount = Math.Max(0, maxRefundable - paidAmount);

            var balance = new SalesReturnBalanceDto(
                salesReturn.SalesReturnId,
                salesReturn.TotalAmount,
                paidAmount,
                remainingAmount);

            return ServiceResult<SalesReturnBalanceDto>.Ok(balance);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting sales return balance for {SalesReturnId}", salesReturnId);
            return ServiceResult<SalesReturnBalanceDto>.Fail(ServiceErrorType.ServerError, $"Error getting sales return balance: {e.Message}");
        }
    }

    private async Task<decimal> ComputeMaxRefundableAsync(Domain.Entities.SalesReturn salesReturn, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(salesReturn.SaleId, cancellationToken);
        if (sale is null)
            return 0;

        var totalPaidOnSale = await paymentRepository.GetTotalAmountByReferenceAsync(
            PaymentReferenceType.SALE, salesReturn.SaleId, cancellationToken);

        var goodsKept = Math.Max(0, sale.TotalAmount - salesReturn.TotalAmount);
        var overpaid = totalPaidOnSale - goodsKept;

        return Math.Max(0, overpaid);
    }
}
