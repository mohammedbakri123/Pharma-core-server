using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class GetSalesSummaryService : IGetSalesSummaryService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISalesReturnRepository _salesReturnRepository;

    public GetSalesSummaryService(
        ISaleRepository saleRepository,
        IPaymentRepository paymentRepository,
        ISalesReturnRepository salesReturnRepository)
    {
        _saleRepository = saleRepository;
        _paymentRepository = paymentRepository;
        _salesReturnRepository = salesReturnRepository;
    }

    public async Task<ServiceResult<SalesSummaryDto>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var totalSales = await _saleRepository.GetTotalSalesAmountByCustomerIdAsync(customerId, cancellationToken);
        
        var sales = await _saleRepository.GetByCustomerIdAsync(customerId, null, null, cancellationToken);
        var saleIds = sales.Select(s => s.SaleId).ToList();
        
        decimal totalPaid = 0;
        foreach (var saleId in saleIds)
        {
            totalPaid += await _paymentRepository.GetTotalAmountByReferenceAsync(PaymentReferenceType.SALE, saleId, cancellationToken);
        }

        var totalReturns = await _salesReturnRepository.GetTotalAmountByCustomerIdAsync(customerId, cancellationToken);

        var summary = new SalesSummaryDto(
            customerId,
            totalSales,
            totalPaid,
            totalReturns,
            totalSales - totalPaid - totalReturns);

        return ServiceResult<SalesSummaryDto>.Ok(summary);
    }
}
