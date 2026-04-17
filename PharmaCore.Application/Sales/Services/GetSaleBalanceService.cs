using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class GetSaleBalanceService : IGetSaleBalanceService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetSaleBalanceService> _logger;

    public GetSaleBalanceService(ISaleRepository saleRepository, IPaymentRepository paymentRepository, ILogger<GetSaleBalanceService> logger)
    {
        _saleRepository = saleRepository;
        _paymentRepository = paymentRepository;
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
            var balance = new SaleBalanceDto(sale.SaleId, sale.TotalAmount, paidAmount, sale.TotalAmount - paidAmount);
            return ServiceResult<SaleBalanceDto>.Ok(balance);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting sale balance for {SaleId}", saleId);
            return ServiceResult<SaleBalanceDto>.Fail(ServiceErrorType.ServerError, $"Error getting sale balance: {e.Message}");
        }
    }
}