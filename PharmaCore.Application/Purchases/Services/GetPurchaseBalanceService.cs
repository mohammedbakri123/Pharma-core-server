using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class GetPurchaseBalanceService : IGetPurchaseBalanceService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPurchaseBalanceService> _logger;

    public GetPurchaseBalanceService(
        IPurchaseRepository purchaseRepository,
        IPaymentRepository paymentRepository,
        ILogger<GetPurchaseBalanceService> logger)
    {
        _purchaseRepository = purchaseRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PurchaseBalanceDto>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var purchase = await _purchaseRepository.GetByIdAsync(purchaseId, cancellationToken);
            if (purchase is null)
                return ServiceResult<PurchaseBalanceDto>.Fail(ServiceErrorType.NotFound, "Purchase not found.");

            var paidAmount = await _paymentRepository.GetTotalAmountByReferenceAsync(
                PaymentReferenceType.PURCHASE, purchaseId, cancellationToken);

            var remainingAmount = purchase.TotalAmount - paidAmount;
            if (remainingAmount < 0) remainingAmount = 0;

            return ServiceResult<PurchaseBalanceDto>.Ok(new PurchaseBalanceDto(
                purchase.PurchaseId,
                purchase.TotalAmount,
                paidAmount,
                remainingAmount));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting balance for purchase {PurchaseId}", purchaseId);
            return ServiceResult<PurchaseBalanceDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
