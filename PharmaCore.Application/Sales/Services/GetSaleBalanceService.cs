using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Services;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class GetSaleBalanceService : IGetSaleBalanceService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly ILogger<GetSaleBalanceService> _logger;

    public GetSaleBalanceService(
        ISaleRepository saleRepository,
        IPaymentRepository paymentRepository,
        ISalesReturnRepository salesReturnRepository,
        ILogger<GetSaleBalanceService> logger)
    {
        _saleRepository = saleRepository;
        _paymentRepository = paymentRepository;
        _salesReturnRepository = salesReturnRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SaleBalanceDto>> ExecuteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await _saleRepository.GetByIdAsync(saleId, cancellationToken);
            if (sale is null)
                return ServiceResult<SaleBalanceDto>.Fail(ServiceErrorType.NotFound, "Sale not found.");

            var paidAmount = await _paymentRepository.GetTotalAmountByReferenceAsync(PaymentReferenceType.SALE, saleId, cancellationToken);
            var returnedAmount = await _salesReturnRepository.GetTotalAmountBySaleIdAsync(saleId, cancellationToken);
            var remaining = PaymentCalculations.ComputeSaleRemaining(sale.TotalAmount, paidAmount, sale.Discount, returnedAmount);
            var balance = new SaleBalanceDto(sale.SaleId, sale.TotalAmount, paidAmount, sale.Discount, returnedAmount, remaining);
            return ServiceResult<SaleBalanceDto>.Ok(balance);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting sale balance for {SaleId}", saleId);
            return ServiceResult<SaleBalanceDto>.Fail(ServiceErrorType.ServerError, $"Error getting sale balance: {e.Message}");
        }
    }
}