using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Services;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class GetSaleBalanceService(
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository,
    ISalesReturnRepository salesReturnRepository,
    ILogger<GetSaleBalanceService> logger)
    : IGetSaleBalanceService
{
    public async Task<ServiceResult<SaleBalanceDto>> ExecuteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken);
            if (sale is null)
                return ServiceResult<SaleBalanceDto>.Fail(ServiceErrorType.NotFound, "Sale not found.");

            var paidAmount = await paymentRepository.GetTotalAmountByReferenceAsync(PaymentReferenceType.SALE, saleId, cancellationToken);
            var returnedAmount = await salesReturnRepository.GetTotalAmountBySaleIdAsync(saleId, cancellationToken);
            var refundedAmount = await paymentRepository.GetTotalRefundedBySaleIdAsync(saleId, cancellationToken);
            var remaining = PaymentCalculations.ComputeSaleRemaining(sale.TotalAmount, paidAmount, sale.Discount, returnedAmount, refundedAmount);
            var balance = new SaleBalanceDto(sale.SaleId, sale.TotalAmount, paidAmount, sale.Discount, returnedAmount, refundedAmount, remaining);
            return ServiceResult<SaleBalanceDto>.Ok(balance);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting sale balance for {SaleId}", saleId);
            return ServiceResult<SaleBalanceDto>.Fail(ServiceErrorType.ServerError, $"Error getting sale balance: {e.Message}");
        }
    }
}